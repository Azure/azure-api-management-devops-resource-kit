using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;
using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class BackendExtractor : EntityExtractor
    {
        public async Task<string> GetBackends(string ApiManagementName, string ResourceGroupName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/backends?api-version={4}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, GlobalConstants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }

        public async Task<string> GetBackend(string ApiManagementName, string ResourceGroupName, string backendName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/backends/{4}?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, backendName, GlobalConstants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }

        public async Task<Template> GenerateBackendsARMTemplate(string apimname, string resourceGroup, string singleApiName, List<TemplateResource> armTemplateResources)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Getting backends from service");
            Template armTemplate = GenerateEmptyTemplateWithParameters();

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // isolate api and operation policy resources in the case of a single api extraction, as they may reference backends
            var policyResources = armTemplateResources.Where(resource => (resource.type == ResourceTypeConstants.APIPolicy || resource.type == ResourceTypeConstants.APIOperationPolicy));

            string backends = await GetBackends(apimname, resourceGroup);
            JObject oBackends = JObject.Parse(backends);

            foreach (var item in oBackends["value"])
            {
                string backendName = ((JValue)item["name"]).Value.ToString();
                string backend = await GetBackend(apimname, resourceGroup, backendName);

                BackendTemplateResource backendTemplateResource = JsonConvert.DeserializeObject<BackendTemplateResource>(backend);
                backendTemplateResource.name = $"[concat(parameters('ApimServiceName'), '/{backendName}')]";
                backendTemplateResource.apiVersion = "2018-06-01-preview";

                // extract all the backends in both cases for the time being
                Console.WriteLine("'{0}' Backend found", backendName);
                templateResources.Add(backendTemplateResource);

                // only extract the backend if this is a full extraction, or in the case of a single api, if it is referenced by one of the policies
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
                //        if (policyContent.Contains(backendName) || policyContent.Contains(backendTemplateResource.properties.url) || policyContent.Contains(backendTemplateResource.properties.resourceId))
                //        {
                //            isReferencedInPolicy = true;
                //        }
                //    }
                //    if (isReferencedInPolicy == true)
                //    {
                //        // backend was used in policy, extract it
                //        Console.WriteLine("'{0}' Backend found", backendName);
                //        templateResources.Add(backendTemplateResource);
                //    }
                //}
            }

            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}
