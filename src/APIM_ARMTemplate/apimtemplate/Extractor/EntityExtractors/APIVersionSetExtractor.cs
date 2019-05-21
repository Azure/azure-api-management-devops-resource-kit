using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;
using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class APIVersionSetExtractor : EntityExtractor
    {
        public async Task<string> GetAPIVersionSetsAsync(string ApiManagementName, string ResourceGroupName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apiVersionSets?api-version={4}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetAPIVersionSetDetailsAsync(string ApiManagementName, string ResourceGroupName, string APIVersionSetName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apiVersionSets/{4}?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, APIVersionSetName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<Template> GenerateAPIVersionSetsARMTemplateAsync(string apimname, string resourceGroup, string singleApiName, List<TemplateResource> apiTemplateResources, string policyXMLBaseUrl)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting API version sets from service");
            Template armTemplate = GenerateEmptyTemplateWithParameters(policyXMLBaseUrl);

            // isolate apis in the case of a single api extraction
            var apiResources = apiTemplateResources.Where(resource => resource.type == ResourceTypeConstants.API);

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // pull all version sets for service
            string versionSets = await GetAPIVersionSetsAsync(apimname, resourceGroup);
            JObject oVersionSets = JObject.Parse(versionSets);

            foreach (var item in oVersionSets["value"])
            {
                string versionSetName = ((JValue)item["name"]).Value.ToString();
                string versionSetDetails = await GetAPIVersionSetDetailsAsync(apimname, resourceGroup, versionSetName);

                // convert returned product to template resource class
                APIVersionSetTemplateResource versionSetTemplateResource = JsonConvert.DeserializeObject<APIVersionSetTemplateResource>(versionSetDetails);
                versionSetTemplateResource.name = $"[concat(parameters('ApimServiceName'), '/{versionSetName}')]";
                versionSetTemplateResource.apiVersion = GlobalConstants.APIVersion;

                // only extract the product if this is a full extraction, or in the case of a single api, if it is found in products associated with the api
                if (singleApiName == null || apiResources.SingleOrDefault(api => (api as APITemplateResource).properties.apiVersionSetId != null && (api as APITemplateResource).properties.apiVersionSetId.Contains(versionSetName)) != null)
                {
                    Console.WriteLine("'{0}' API version set found", versionSetName);
                    templateResources.Add(versionSetTemplateResource);
                }
            }

            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}
