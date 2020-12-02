using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class PropertyExtractor : EntityExtractor
    {
        public async Task<string> GetPropertyByDisplayName(string ApiManagementName, string ResourceGroupName, string displayName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();
            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/properties?api-version={4}&$filter={5}",
                baseUrl, azSubId, ResourceGroupName, ApiManagementName, GlobalConstants.APIVersion, HttpUtility.UrlEncode($"properties/displayName eq '{displayName}'"));

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetPropertiesAsync(string ApiManagementName, string ResourceGroupName, string singleApiName = null)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            if (singleApiName == null)
            {
                string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/properties?api-version={4}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, GlobalConstants.APIVersion);

                return await CallApiManagementAsync(azToken, requestUrl);
            }
            else
            {   
                JArray policies = new JArray();

                // Get API Extractor for reading policies
                FileWriter fw = new FileWriter();
                var apiExtractor = new APIExtractor(fw);

                // Add API policy
                policies.Add(await apiExtractor.GetAPIPolicyAsync(ApiManagementName, ResourceGroupName, singleApiName));

                // Get all operations of a specific API
                var operations = await apiExtractor.GetAllOperationNames(ApiManagementName, ResourceGroupName, singleApiName);
                
                // Add Operation policies
                foreach (var operation in operations)
                {
                    policies.Add(await apiExtractor.GetOperationPolicyAsync(ApiManagementName, ResourceGroupName, singleApiName, operation));
                }

                // Extract DisplayNames
                JArray nvDisplayNames = new JArray();
                var propertyRegex = new Regex("{{(.*?)}}");
                foreach (var policy in policies)
                {
                    var matches = propertyRegex.Matches(policy?.ToString());

                    foreach (Match match in matches)
                    {
                        nvDisplayNames.Add(match.Groups[1].Value);
                    }
                }

                // Get named value ids from displayNames
                var namedValues = new JObject();
                namedValues["value"] = new JArray();
                foreach (var displayName in nvDisplayNames)
                {
                    var nv = await GetPropertyByDisplayName(ApiManagementName, ResourceGroupName, displayName?.ToString());
                    var nvO = JObject.Parse(nv);
                    ((JArray)namedValues["value"]).Add(nvO["value"][0]);
                }

                namedValues["count"] = ((JArray)namedValues["value"]).Count;

                return namedValues.ToString();
            }
        }

        public async Task<string> GetPropertyDetailsAsync(string ApiManagementName, string ResourceGroupName, string propertyName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/properties/{4}?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, propertyName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<Template> GenerateNamedValuesTemplateAsync(string singleApiName, List<TemplateResource> apiTemplateResources, Extractor exc)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting named values from service");
            Template armTemplate = GenerateEmptyPropertyTemplateWithParameters(exc);

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // pull named values (properties) for service
            string properties = await GetPropertiesAsync(exc.sourceApimName, exc.resourceGroup, singleApiName);
            JObject oProperties = JObject.Parse(properties);

            foreach (var extractedProperty in oProperties["value"])
            {
                string propertyName = ((JValue)extractedProperty["name"]).Value.ToString();
                string fullPropertyResource = await GetPropertyDetailsAsync(exc.sourceApimName, exc.resourceGroup, propertyName);

                // convert returned named value to template resource class
                PropertyTemplateResource propertyTemplateResource = GetPropertyTemplateResource(propertyName, fullPropertyResource);

                if (exc.paramNamedValue)
                {
                    propertyTemplateResource.properties.value = $"[parameters('{ParameterNames.NamedValues}').{ExtractorUtils.GenValidParamName(propertyName, ParameterPrefix.Property)}]";
                }

                if (singleApiName == null)
                {
                    // if the user is executing a full extraction, extract all the loggers
                    Console.WriteLine("'{0}' Named value found", propertyName);
                    templateResources.Add(propertyTemplateResource);
                }
                else
                {
                    // TODO - if the user is executing a single api, extract all the named values used in the template resources
                    Console.WriteLine("'{0}' Named value found", propertyName);
                    templateResources.Add(propertyTemplateResource);
                };
            }

            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }

        public PropertyTemplateResource GetPropertyTemplateResource(string propertyName, string fullPropertyResource)
        {
            PropertyTemplateResource propertyTemplateResource = JsonConvert.DeserializeObject<PropertyTemplateResource>(fullPropertyResource);
            propertyTemplateResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{propertyName}')]";
            propertyTemplateResource.type = ResourceTypeConstants.Property;
            propertyTemplateResource.apiVersion = GlobalConstants.APIVersion;
            propertyTemplateResource.scale = null;
            return propertyTemplateResource;
        }
    }
}
