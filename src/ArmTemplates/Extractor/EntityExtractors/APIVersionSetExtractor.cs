using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;
using System;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class ApiVersionSetExtractor : EntityExtractorBase, IApiVersionSetExtractor
    {
        public async Task<Template> GenerateAPIVersionSetsARMTemplateAsync(string apimname, string resourceGroup, string singleApiName, List<TemplateResource> apiTemplateResources)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting API version sets from service");
            Template armTemplate = this.GenerateTemplateWithApimServiceNameProperty();

            // isolate apis in the case of a single api extraction
            var apiResources = apiTemplateResources.Where(resource => resource.Type == ResourceTypeConstants.API);

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // pull all version sets for service
            string versionSets = await this.GetAPIVersionSetsAsync(apimname, resourceGroup);
            JObject oVersionSets = JObject.Parse(versionSets);

            foreach (var item in oVersionSets["value"])
            {
                string versionSetName = ((JValue)item["name"]).Value.ToString();
                string versionSetDetails = await this.GetAPIVersionSetDetailsAsync(apimname, resourceGroup, versionSetName);

                // convert returned product to template resource class
                APIVersionSetTemplateResource versionSetTemplateResource = JsonConvert.DeserializeObject<APIVersionSetTemplateResource>(versionSetDetails);
                versionSetTemplateResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{versionSetName}')]";
                versionSetTemplateResource.ApiVersion = GlobalConstants.ApiVersion;

                // only extract the product if this is a full extraction, or in the case of a single api, if it is found in products associated with the api
                if (singleApiName == null || apiResources.SingleOrDefault(api => (api as APITemplateResource).Properties.ApiVersionSetId != null && (api as APITemplateResource).Properties.ApiVersionSetId.Contains(versionSetName)) != null)
                {
                    Console.WriteLine("'{0}' API version set found", versionSetName);
                    templateResources.Add(versionSetTemplateResource);
                }
            }

            armTemplate.Resources = templateResources.ToArray();
            return armTemplate;
        }

        async Task<string> GetAPIVersionSetsAsync(string apiManagementName, string resourceGroupName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apiVersionSets?api-version={4}",
               BaseUrl, azSubId, resourceGroupName, apiManagementName, GlobalConstants.ApiVersion);

            return await this.CallApiManagementAsync(azToken, requestUrl);
        }

        async Task<string> GetAPIVersionSetDetailsAsync(string apiManagementName, string resourceGroupName, string apiVersionSetName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apiVersionSets/{4}?api-version={5}",
               BaseUrl, azSubId, resourceGroupName, apiManagementName, apiVersionSetName, GlobalConstants.ApiVersion);

            return await this.CallApiManagementAsync(azToken, requestUrl);
        }
    }
}
