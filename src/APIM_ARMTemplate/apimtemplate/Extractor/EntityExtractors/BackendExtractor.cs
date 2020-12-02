using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class BackendExtractor : EntityExtractor
    {
        public async Task<string> GetBackendsAsync(string ApiManagementName, string ResourceGroupName, string singleApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();
            if (string.IsNullOrEmpty(singleApiName))
            {
                string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/backends?api-version={4}",
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
                JArray backendIds = new JArray();
                var propertyRegex = new Regex("backend-id=\"({{(.*?)}}|.*?)\"");
                foreach (var policy in policies)
                {
                    var policyO = JObject.Parse(policy?.ToString());
                    var policyString = policyO["properties"]["value"]?.ToString();
                    var matches = propertyRegex.Matches(policyString);

                    foreach (Match match in matches)
                    {
                        string value = match.Groups[1].Value;

                        // Handle Named Value
                        if (value.StartsWith("{{"))
                        {
                            var nvExtractor = new PropertyExtractor();

                            // Get NamedValue without braces
                            value = match.Groups[2].Value;
                            var property = await nvExtractor.GetPropertyByDisplayName(ApiManagementName, ResourceGroupName, value);
                            var propertyO = JObject.Parse(property);
                            var backendId = propertyO["value"][0]["properties"]["value"]?.ToString();
                            backendIds.Add(backendId);
                        }
                        else
                        {
                            backendIds.Add(value);
                        }
                    }
                }

                var backends = new JObject();
                backends["value"] = new JArray();
                foreach (var backendId in backendIds)
                {
                    var backend = await GetBackendDetailsAsync(ApiManagementName, ResourceGroupName, backendId?.ToString());
                    var backendO = JObject.Parse(backend);
                    ((JArray)backends["value"]).Add(backendO);
                }

                backends["count"] = ((JArray)backends["value"]).Count;

                return backends.ToString();
            }
        }

        public async Task<string> GetBackendDetailsAsync(string ApiManagementName, string ResourceGroupName, string backendName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/backends/{4}?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, backendName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<Template> GenerateBackendsARMTemplateAsync(string apimname, string resourceGroup, string singleApiName, List<TemplateResource> apiTemplateResources, List<TemplateResource> propertyResources, string policyXMLBaseUrl, string policyXMLSasToken)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting backends from service");
            Template armTemplate = GenerateEmptyTemplateWithParameters(policyXMLBaseUrl, policyXMLSasToken);

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // isolate api and operation policy resources in the case of a single api extraction, as they may reference backends
            var policyResources = apiTemplateResources.Where(resource => (resource.type == ResourceTypeConstants.APIPolicy || resource.type == ResourceTypeConstants.APIOperationPolicy || resource.type == ResourceTypeConstants.ProductPolicy));
            var namedValueResources = propertyResources.Where(resource => (resource.type == ResourceTypeConstants.Property));

            // pull all backends for service
            string backends = await GetBackendsAsync(apimname, resourceGroup, singleApiName);
            JObject oBackends = JObject.Parse(backends);

            foreach (var item in oBackends["value"])
            {
                string backendName = ((JValue)item["name"]).Value.ToString();
                string backend = await GetBackendDetailsAsync(apimname, resourceGroup, backendName);

                // convert returned backend to template resource class
                BackendTemplateResource backendTemplateResource = JsonConvert.DeserializeObject<BackendTemplateResource>(backend);
                backendTemplateResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{backendName}')]";
                backendTemplateResource.apiVersion = GlobalConstants.APIVersion;

                if (singleApiName != null)
                {
                    // Extract Properties
                    JArray namedValues = new JArray();
                    var propertyRegex = new Regex("{{(.*?)}}");
                    var matches = propertyRegex.Matches(item?.ToString());
                    foreach (Match match in matches)
                    {
                        var propertyName = match.Groups[1].Value;
                        var propertyExtractor = new PropertyExtractor();
                        var fullPropertyResource = await propertyExtractor.GetPropertyDetailsAsync(apimname, resourceGroup, propertyName);
                        PropertyTemplateResource propertyTemplateResource = propertyExtractor.GetPropertyTemplateResource(propertyName, fullPropertyResource);
                        propertyResources.Add(propertyTemplateResource);
                    }
                }

                ////only extract the backend if this is a full extraction, or in the case of a single api, if it is referenced by one of the policies
                //if (singleApiName == null)
                //{
                //    // if the user is extracting all apis, extract all the backends
                //    Console.WriteLine("'{0}' Backend found", backendName);
                //    templateResources.Add(backendTemplateResource);
                //}
                //else
                //{
                //    bool isReferencedInPolicy = false;
                //    foreach (PolicyTemplateResource policyTemplateResource in policyResources)
                //    {
                //        // the backend is used in a policy if the xml contains a set-backend-service policy, which will reference the backend's url or id
                //        string policyContent = policyTemplateResource.properties.policyContent;
                //        isReferencedInPolicy = DoesPolicyReferenceBackend(policyContent, namedValueResources,  backendName, backendTemplateResource);
                //    }
                //    if (isReferencedInPolicy == true)
                //    {
                //        // backend was used in policy, extract it
                //        Console.WriteLine("'{0}' Backend found", backendName);
                //        templateResources.Add(backendTemplateResource);
                //    }
                //}

                Console.WriteLine("'{0}' Backend found", backendName);
                templateResources.Add(backendTemplateResource);
            }

            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }

        public bool DoesPolicyReferenceBackend(string policyContent, IEnumerable<TemplateResource> namedValueResources, string backendName, BackendTemplateResource backendTemplateResource)
        {
            // a policy is referenced by a backend with the set-backend-service policy, which will reference use the backends name or url, or through referencing a named value that applies to the backend
            var namedValueResourcesUsedByBackend = namedValueResources.Where(resource => DoesBackendReferenceNamedValue(resource, backendTemplateResource));
            if ((backendName != null && policyContent.Contains(backendName)) || 
                (backendTemplateResource.properties.url != null && policyContent.Contains(backendTemplateResource.properties.url)) || 
                (backendTemplateResource.properties.title != null && policyContent.Contains(backendTemplateResource.properties.title)) ||
                (backendTemplateResource.properties.resourceId != null && policyContent.Contains(backendTemplateResource.properties.resourceId)))
            {
                return true;
            }
            foreach(PropertyTemplateResource namedValueResource in namedValueResourcesUsedByBackend)
            {
                if (policyContent.Contains(namedValueResource.properties.displayName) || policyContent.Contains(namedValueResource.properties.value))
                {
                    return true;
                }

            }
            return false;
        }

        public bool DoesBackendReferenceNamedValue(TemplateResource namedValueResource, BackendTemplateResource backendTemplateResource)
        {
            string namedValue = (namedValueResource as PropertyTemplateResource).properties.value;
            return (namedValue == backendTemplateResource.properties.url 
                || namedValue == backendTemplateResource.properties.description
                || namedValue == backendTemplateResource.properties.title);
        }
    }
}
