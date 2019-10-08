using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class TagExtractor: EntityExtractor
    {
        public async Task<string> GetTagsAsync(string ApiManagementName, string ResourceGroupName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/Tags?api-version={4}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<Template> GenerateTagsTemplateAsync(string apimname, string resourceGroup, string singleApiName, List<TemplateResource> apiTemplateResources, string policyXMLBaseUrl)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting tags from service");
            Template armTemplate = GenerateEmptyTemplateWithParameters(policyXMLBaseUrl);

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // pull all named values (Tags) for service
            string Tags = await GetTagsAsync(apimname, resourceGroup);
            JObject oTags = JObject.Parse(Tags);

            foreach (var extractedTag in oTags["value"])
            {
                string TagName = ((JValue)extractedTag["name"]).Value.ToString();
                
                // convert returned named value to template resource class
                TagTemplateResource TagTemplateResource = JsonConvert.DeserializeObject<TagTemplateResource>(extractedTag.ToString());
                TagTemplateResource.name = $"[concat(parameters('ApimServiceName'), '/{TagName}')]";
                TagTemplateResource.type = ResourceTypeConstants.Tag;
                TagTemplateResource.apiVersion = GlobalConstants.APIVersion;
                TagTemplateResource.scale = null;

                if (singleApiName == null)
                {
                    // if the user is executing a full extraction, extract all the tags
                    Console.WriteLine("'{0}' Tag found", TagName);
                    templateResources.Add(TagTemplateResource);
                }
                else
                {
                    // TODO - if the user is executing a single api, extract all the tags used in the template resources
                    Console.WriteLine("'{0}' Tag found", TagName);
                    templateResources.Add(TagTemplateResource);
                };
            }

            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}
