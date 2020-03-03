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
        
        /// <summary>
        /// Creates the Tags request URL based on the Azure subscription ID, Resource Group name, and APIM instance name.
        /// </summary>
        /// <param name="ApiManagementName"></param>
        /// <param name="ResourceGroupName"></param>
        /// <returns>A string representing a request URL</returns>
        /// <remarks>
        /// This was split out from the GetTagsAsync method in order to make repeated calls with "nextLink" URLs.
        /// </remarks>
        protected async Task<string> GetBaseTagsUrlAsync(string ApiManagementName, string ResourceGroupName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            return string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/Tags?api-version={4}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, GlobalConstants.APIVersion);
        }

        /// <summary>
        /// Makes an Azure API Management REST API call
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <returns>a JSON string with the API call results</returns>
        /// <remarks>
        /// Request URL creation was moved out of here in order to make repeated calls to this method with different URLs.
        /// Changed to protected because I didn't see a reason for it to be public.
        /// </remarks>
        protected async Task<string> GetTagsAsync(string requestUrl)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();
            return await CallApiManagementAsync(azToken, requestUrl);
        }

        /// <summary>
        /// Creates tag association resources and adds them to the referenced templateResources collection.
        /// </summary>
        /// <param name="oTags"></param>
        /// <param name="singleApiName"></param>
        /// <param name="apiTemplateResources"></param>
        /// <param name="productTemplateResources"></param>
        /// <param name="templateResources"></param>
        /// <remarks>
        /// This is basically an encapsulation of the original foreach loop.
        /// It was refactored out in order to call it from a loop for each set of API results.
        /// </remarks>
        protected void AddTagAssociationTemplateResources( JObject oTags, string singleApiName, List<TemplateResource> apiTemplateResources, 
            List<TemplateResource> productTemplateResources, ref List<TemplateResource> templateResources )
        {
            // isolate tag and api operation associations in the case of a single api extraction
            var apiOperationTagResources = apiTemplateResources.Where(resource => resource.type == ResourceTypeConstants.APIOperationTag);

            // isolate tag and api associations in the case of a single api extraction
            var apiTagResources = apiTemplateResources.Where(resource => resource.type == ResourceTypeConstants.APITag);

            // isolate product api associations in the case of a single api extraction
            var productAPIResources = apiTemplateResources.Where(resource => resource.type == ResourceTypeConstants.ProductAPI);

            // isolate tag and product associations in the case of a single api extraction
            var productTagResources = productTemplateResources.Where(resource => resource.type == ResourceTypeConstants.ProductTag);

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
        }

        /// <summary>
        /// Creates resource Template
        /// </summary>
        /// <param name="apimname"></param>
        /// <param name="resourceGroup"></param>
        /// <param name="singleApiName"></param>
        /// <param name="apiTemplateResources"></param>
        /// <param name="productTemplateResources"></param>
        /// <param name="policyXMLBaseUrl"></param>
        /// <param name="policyXMLSasToken"></param>
        /// <returns>The contexts for an ARM template</returns>
        /// <remarks>
        /// This method starts with the request URL the way that it was originally built, determines the tags associations, and builds the ARM resources.
        /// Additionally, if "nextLink" in the API results is not null, loops back to repeat the process with the results from the "nextLink" query.
        /// Exits the while loop once no more results are available.
        /// </remarks>
        public async Task<Template> GenerateTagsTemplateAsync(string apimname, string resourceGroup, string singleApiName, List<TemplateResource> apiTemplateResources, List<TemplateResource> productTemplateResources, string policyXMLBaseUrl, string policyXMLSasToken)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting tags from service");
            Template armTemplate = GenerateEmptyTemplateWithParameters(policyXMLBaseUrl, policyXMLSasToken);

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // pull all named values (Tags) for service
            string requestUrl = await GetBaseTagsUrlAsync(apimname, resourceGroup);

            while( ! string.IsNullOrEmpty( requestUrl ))
            {
                string Tags = await GetTagsAsync(requestUrl);
                JObject oTags = JObject.Parse(Tags);

                AddTagAssociationTemplateResources(oTags, singleApiName, apiTemplateResources, productTemplateResources, ref templateResources);

                JValue nextLink = (JValue)oTags["nextLink"];
                requestUrl = (( null == nextLink ) ? string.Empty : nextLink.Value.ToString() ); 
            }

            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}