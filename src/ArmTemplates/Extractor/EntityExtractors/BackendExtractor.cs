using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;
using System;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class BackendExtractor : EntityExtractorBase, IBackendExtractor
    {
        public async Task<string> GetBackendsAsync(string ApiManagementName, string ResourceGroupName, int skipNumOfRecords)
        {
            (string azToken, string azSubId) = await Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/backends?$skip={4}&api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, skipNumOfRecords, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetBackendDetailsAsync(string ApiManagementName, string ResourceGroupName, string backendName)
        {
            (string azToken, string azSubId) = await Auth.GetAccessToken();

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
        public async Task<Tuple<Template, Dictionary<string, BackendApiParameters>>> GenerateBackendsARMTemplateAsync(string apimname, string resourceGroup, string singleApiName, List<TemplateResource> apiTemplateResources, List<TemplateResource> propertyResources, ExtractorParameters extractorParameters)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting backends from service");
            Template armTemplate = GenerateEmptyPropertyTemplateWithParameters();

            if (extractorParameters.paramBackend)
            {
                TemplateParameterProperties extractBackendParametersProperties = new TemplateParameterProperties()
                {
                    type = "object"
                };
                armTemplate.parameters.Add(ParameterNames.BackendSettings, extractBackendParametersProperties);
            }

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // isolate api and operation policy resources in the case of a single api extraction, as they may reference backends
            var policyResources = apiTemplateResources.Where(resource => resource.type == ResourceTypeConstants.APIPolicy || resource.type == ResourceTypeConstants.APIOperationPolicy || resource.type == ResourceTypeConstants.ProductPolicy);
            var namedValueResources = propertyResources.Where(resource => resource.type == ResourceTypeConstants.Property);

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
                            string policyContent = PolicyTemplateUtils.GetPolicyContent(extractorParameters, policyTemplateResource);

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
                        if (extractorParameters.paramBackend)
                        {
                            var apiToken = new BackendApiParameters();
                            string validApiParamName = ParameterNamingHelper.GenerateValidParameterName(backendName, ParameterPrefix.Diagnostic).ToLower();

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
            if (backendName != null && policyContent.Contains(backendName) ||
                backendTemplateResource.properties.url != null && policyContent.Contains(backendTemplateResource.properties.url) ||
                backendTemplateResource.properties.title != null && policyContent.Contains(backendTemplateResource.properties.title) ||
                backendTemplateResource.properties.resourceId != null && policyContent.Contains(backendTemplateResource.properties.resourceId))
            {
                return true;
            }
            foreach (PropertyTemplateResource namedValueResource in namedValueResourcesUsedByBackend)
            {
                if (namedValueResource.properties.displayName != null && policyContent.Contains(namedValueResource.properties.displayName) ||
                    namedValueResource.properties.value != null && policyContent.Contains(namedValueResource.properties.value))
                {
                    return true;
                }

            }
            return false;
        }

        public bool DoesBackendReferenceNamedValue(TemplateResource namedValueResource, BackendTemplateResource backendTemplateResource)
        {
            string namedValue = (namedValueResource as PropertyTemplateResource).properties.value;
            return namedValue == backendTemplateResource.properties.url
                || namedValue == backendTemplateResource.properties.description
                || namedValue == backendTemplateResource.properties.title;
        }

        public async Task<bool> IsNamedValueUsedInBackends(string apimname, string resourceGroup, string singleApiName, List<TemplateResource> apiTemplateResources, ExtractorParameters extractorParameters, string propertyName, string propertyDisplayName)
        {
            // isolate api and operation policy resources in the case of a single api extraction, as they may reference backends
            var policyResources = apiTemplateResources.Where(resource => resource.type == ResourceTypeConstants.APIPolicy || resource.type == ResourceTypeConstants.APIOperationPolicy || resource.type == ResourceTypeConstants.ProductPolicy);
            var emptyNamedValueResources = new List<TemplateResource>();

            // pull all backends for service
            JObject oBackends = new JObject();
            int skipNumberOfBackends = 0;

            do
            {
                string backends = await GetBackendsAsync(apimname, resourceGroup, skipNumberOfBackends);
                oBackends = JObject.Parse(backends);

                foreach (var item in oBackends["value"])
                {
                    var content = item.ToString();

                    // check if backend references the named value, credentials for example
                    if (content.Contains(string.Concat("{{", propertyName, "}}")) || content.Contains(string.Concat("{{", propertyDisplayName, "}}")))
                    {
                        //only true if this is a full extraction, or in the case of a single api, if it is referenced by one of the API policies
                        if (singleApiName == null)
                        {
                            return true;
                        }
                        else
                        {
                            // is this backend related to the single api?
                            // is backend used in the extracted policies for this API
                            // if backend id is referenced in policy
                            // or a named value is referenced in policy to a backend, we have already checked the policy for named value.

                            // check if this backend is used by any of the policies extracted
                            string backendName = ((JValue)item["name"]).Value.ToString();
                            string backend = await GetBackendDetailsAsync(apimname, resourceGroup, backendName);

                            // convert returned backend to template resource class
                            BackendTemplateResource backendTemplateResource = JsonConvert.DeserializeObject<BackendTemplateResource>(backend);

                            // we have already checked if the named value is used in a policy, we just need to confirm if the backend is referenced by this single api within the policy file
                            // this is why an empty named values must be passed to this method for validation
                            foreach (PolicyTemplateResource policyTemplateResource in policyResources)
                            {
                                string policyContent = PolicyTemplateUtils.GetPolicyContent(extractorParameters, policyTemplateResource);

                                if (DoesPolicyReferenceBackend(policyContent, emptyNamedValueResources, backendName, backendTemplateResource))
                                {
                                    // dont need to go through all policies and backends if the named values has already been found
                                    return true;
                                }
                            }
                        }
                    }
                }

                skipNumberOfBackends += GlobalConstants.NumOfRecords;
            }
            while (oBackends["nextLink"] != null);

            return false;
        }
    }
}
