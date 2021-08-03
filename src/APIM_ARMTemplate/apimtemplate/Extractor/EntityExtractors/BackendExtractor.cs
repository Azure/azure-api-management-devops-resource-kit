using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;
using System;
using apimtemplate.Common.TemplateModels;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class BackendExtractor : EntityExtractor
    {
        public async Task<string> GetBackendsAsync(string ApiManagementName, string ResourceGroupName, int skipNumOfRecords)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/backends?$skip={4}&api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, skipNumOfRecords, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetBackendDetailsAsync(string ApiManagementName, string ResourceGroupName, string backendName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/backends/{4}?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, backendName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        /// <summary>
        /// Generate the ARM assets for the backend resources
        /// </summary>
        /// <param name="apimname"></param>
        /// <param name="resourceGroup"></param>
        /// <param name="singleApiName"></param>
        /// <param name="apiTemplateResources"></param>
        /// <param name="propertyResources"></param>
        /// <param name="policyXMLBaseUrl"></param>
        /// <param name="policyXMLSasToken"></param>
        /// <param name="extractBackendParameters"></param>
        /// <returns>a combination of a Template and the value for the BackendSettings parameter</returns>
        public async Task<Tuple<Template,Dictionary<string,BackendApiParameters> > > GenerateBackendsARMTemplateAsync(string apimname, string resourceGroup, string singleApiName, List<TemplateResource> apiTemplateResources, List<TemplateResource> propertyResources, Extractor exc)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting backends from service");
            Template armTemplate = GenerateEmptyPropertyTemplateWithParameters();

            if (exc.paramBackend)
            {
                TemplateParameterProperties extractBackendParametersProperties = new TemplateParameterProperties()
                {
                    type = "object"
                };
                armTemplate.parameters.Add(ParameterNames.BackendSettings, extractBackendParametersProperties);
            }

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // isolate api and operation policy resources in the case of a single api extraction, as they may reference backends
            var policyResources = apiTemplateResources.Where(resource => (resource.type == ResourceTypeConstants.APIPolicy || resource.type == ResourceTypeConstants.APIOperationPolicy || resource.type == ResourceTypeConstants.ProductPolicy));
            var namedValueResources = propertyResources.Where(resource => (resource.type == ResourceTypeConstants.Property));

            // pull all backends for service
            JObject oBackends = new JObject();
            var oBackendParameters = new Dictionary<string, BackendApiParameters>();
            int skipNumberOfBackends = 0;

            do
            {
                string backends = await GetBackendsAsync(apimname, resourceGroup, skipNumberOfBackends);
                oBackends = JObject.Parse(backends);

                foreach (var item in oBackends["value"])
                {
                    string backendName = ((JValue)item["name"]).Value.ToString();
                    string backend = await GetBackendDetailsAsync(apimname, resourceGroup, backendName);

                    // convert returned backend to template resource class
                    BackendTemplateResource backendTemplateResource = JsonConvert.DeserializeObject<BackendTemplateResource>(backend);
                    backendTemplateResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{backendName}')]";
                    backendTemplateResource.apiVersion = GlobalConstants.APIVersion;

                    bool includeBackend = false;
                    ////only extract the backend if this is a full extraction, or in the case of a single api, if it is referenced by one of the policies
                    if (singleApiName == null)
                    {
                        // if the user is extracting all apis, extract all the backends
                        includeBackend = true;
                    }
                    else
                    {
                        // check extracted policies to see if the backend is referenced.
                        foreach (PolicyTemplateResource policyTemplateResource in policyResources)
                        {
                            string policyContent = ExtractorUtils.GetPolicyContent(exc, policyTemplateResource);

                            if (DoesPolicyReferenceBackend(policyContent, namedValueResources, backendName, backendTemplateResource))
                            {
                                // backend was used in policy, extract it
                                includeBackend = true;

                                // dont need to go through all policies if the back end has already been found
                                break;
                            }
                        }
                    }

                    if (includeBackend)
                    {
                        if (exc.paramBackend)
                        {
                            var apiToken = new BackendApiParameters();
                            string validApiParamName = ExtractorUtils.GenValidParamName(backendName, ParameterPrefix.Diagnostic).ToLower();

                            if (!string.IsNullOrEmpty(backendTemplateResource.properties.resourceId))
                            {
                                apiToken.resourceId = backendTemplateResource.properties.resourceId;
                                backendTemplateResource.properties.resourceId = $"[parameters('{ParameterNames.BackendSettings}').{validApiParamName}.resourceId]";
                            }

                            if (!string.IsNullOrEmpty(backendTemplateResource.properties.url))
                            {
                                apiToken.url = backendTemplateResource.properties.url;
                                backendTemplateResource.properties.url = $"[parameters('{ParameterNames.BackendSettings}').{validApiParamName}.url]";
                            }

                            if (!string.IsNullOrEmpty(backendTemplateResource.properties.protocol))
                            {
                                apiToken.protocol = backendTemplateResource.properties.protocol;
                                backendTemplateResource.properties.protocol = $"[parameters('{ParameterNames.BackendSettings}').{validApiParamName}.protocol]";
                            }
                            oBackendParameters.Add(validApiParamName, apiToken);
                        }

                        Console.WriteLine("'{0}' Backend found", backendName);
                        templateResources.Add(backendTemplateResource);
                    }
                }

                skipNumberOfBackends += GlobalConstants.NumOfRecords;
            }
            while (oBackends["nextLink"] != null);

            armTemplate.resources = templateResources.ToArray();
            return new Tuple<Template, Dictionary<string, BackendApiParameters>>(armTemplate, oBackendParameters);
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
            foreach (PropertyTemplateResource namedValueResource in namedValueResourcesUsedByBackend)
            {
                if ((namedValueResource.properties.displayName != null && policyContent.Contains(namedValueResource.properties.displayName)) ||
                    (namedValueResource.properties.value != null && policyContent.Contains(namedValueResource.properties.value)))
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
