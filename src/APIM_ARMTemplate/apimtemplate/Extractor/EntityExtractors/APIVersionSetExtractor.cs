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
        public async Task<string> GetAPIVersionSets(string ApiManagementName, string ResourceGroupName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apiVersionSets?api-version={4}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, GlobalConstants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }

        public async Task<string> GetAPIVersionSetDetails(string ApiManagementName, string ResourceGroupName, string APIVersionSetName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apiVersionSets/{4}?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, APIVersionSetName, GlobalConstants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }

        public async Task<Template> GenerateAPIVersionSetsARMTemplate(string apimname, string resourceGroup, string singleApiName, List<TemplateResource> apiTemplateResources)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Getting API Version Sets from service");
            Template armTemplate = GenerateEmptyTemplateWithParameters();

            // isolate apis in the case of a single api extraction
            var apiResources = apiTemplateResources.Where(resource => resource.type == ResourceTypeConstants.API);

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // pull all version sets for service
            string versionSets = await GetAPIVersionSets(apimname, resourceGroup);
            JObject oVersionSets = JObject.Parse(versionSets);

            foreach (var item in oVersionSets["value"])
            {
                string versionSetName = ((JValue)item["name"]).Value.ToString();
                string versionSetDetails = await GetAPIVersionSetDetails(apimname, resourceGroup, versionSetName);

                // convert returned product to template resource class
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                APIVersionSetTemplateResource versionSetTemplateResource = JsonConvert.DeserializeObject<APIVersionSetTemplateResource>(versionSetDetails, settings);
                versionSetTemplateResource.name = $"[concat(parameters('ApimServiceName'), '/{versionSetName}')]";
                versionSetTemplateResource.apiVersion = GlobalConstants.APIVersion;

                // only extract the product if this is a full extraction, or in the case of a single api, if it is found in products associated with the api
                if (singleApiName == null || apiResources.SingleOrDefault(api => (api as APITemplateResource).properties.apiVersionSetId.Contains(versionSetName)) != null)
                {
                    Console.WriteLine("'{0}' API Version Set found", versionSetName);
                    templateResources.Add(versionSetTemplateResource);
                }
            }

            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}
