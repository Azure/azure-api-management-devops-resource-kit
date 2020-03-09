using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class TagExtractor : EntityExtractor
    {
        public async Task<string> GetTagsAsync(string ApiManagementName, string ResourceGroupName, int skipNumOfRecords )
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/Tags?$skip={4}&api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, skipNumOfRecords, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<Template> GenerateTagsTemplateAsync(string apimname, string resourceGroup, string singleApiName, List<TemplateResource> apiTemplateResources, List<TemplateResource> productTemplateResources, string policyXMLBaseUrl, string policyXMLSasToken)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting tags from service");
            Template armTemplate = GenerateEmptyTemplateWithParameters(policyXMLBaseUrl, policyXMLSasToken);

            // isolate tag and api operation associations in the case of a single api extraction
            var apiOperationTagResources = apiTemplateResources.Where(resource => resource.type == ResourceTypeConstants.APIOperationTag);

            // isolate tag and api associations in the case of a single api extraction
            var apiTagResources = apiTemplateResources.Where(resource => resource.type == ResourceTypeConstants.APITag);

            // isolate product api associations in the case of a single api extraction
            var productAPIResources = apiTemplateResources.Where(resource => resource.type == ResourceTypeConstants.ProductAPI);

            // isolate tag and product associations in the case of a single api extraction
            var productTagResources = productTemplateResources.Where(resource => resource.type == ResourceTypeConstants.ProductTag);

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // pull all named values (Tags) for service
            JObject oTags = new JObject();
            int skipNumOfTags = 0;

            do
            {
                string Tags = await GetTagsAsync(apimname, resourceGroup, skipNumOfTags);
                oTags = JObject.Parse(Tags);

                foreach (var extractedTag in oTags["value"])
                {
                    string TagName = ((JValue)extractedTag["name"]).Value.ToString();

                    // convert returned named value to template resource class
                    TagTemplateResource TagTemplateResource = JsonConvert.DeserializeObject<TagTemplateResource>(extractedTag.ToString());
                    TagTemplateResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{TagName}')]";
                    TagTemplateResource.type = ResourceTypeConstants.Tag;
                    TagTemplateResource.apiVersion = GlobalConstants.APIVersion;
                    TagTemplateResource.scale = null;

                    // only extract the tag if this is a full extraction, 
                    // or in the case of a single api, if it is found in tags associated with the api operations
                    // or if it is found in tags associated with the api
                    // or if it is found in tags associated with the products associated with the api
                    if (singleApiName == null
                            || apiOperationTagResources.Any(t => t.name.Contains($"/{TagName}'"))
                            || apiTagResources.Any(t => t.name.Contains($"/{TagName}'"))
                            || (productAPIResources.Any(t => t.name.Contains($"/{singleApiName}"))
                                && productTagResources.Any(t => t.name.Contains($"/{TagName}'"))))
                    {
                        Console.WriteLine("'{0}' Tag found", TagName);
                        templateResources.Add(TagTemplateResource);
                    }
                }

                skipNumOfTags += GlobalConstants.NumOfRecords;
            }
            while (oTags["nextLink"] != null);


            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}