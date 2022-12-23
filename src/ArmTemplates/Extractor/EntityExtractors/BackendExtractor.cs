// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class BackendExtractor : IBackendExtractor
    {
        readonly ILogger<BackendExtractor> logger;
        readonly ITemplateBuilder templateBuilder;

        readonly IPolicyExtractor policyExtractor;
        readonly IBackendClient backendClient;

        public BackendExtractor(
            ILogger<BackendExtractor> logger,
            ITemplateBuilder templateBuilder,
            IPolicyExtractor policyExtractor,
            IBackendClient backendClient)
        {
            this.logger = logger;
            this.templateBuilder = templateBuilder;
            this.policyExtractor = policyExtractor;
            this.backendClient = backendClient;
        }

        /// <summary>
        /// Generate the ARM assets for the backend resources
        /// </summary>
        /// <returns>a combination of a Template and the value for the BackendSettings parameter</returns>
        public async Task<Template<BackendTemplateResources>> GenerateBackendsTemplateAsync(
            string singleApiName,
            List<PolicyTemplateResource> apiPolicies,
            string baseFilesGenerationDirectory,
            ExtractorParameters extractorParameters)
        {
            var backendTemplate = this.templateBuilder
                                        .GenerateTemplateWithApimServiceNameProperty()
                                        .AddParameterizedBackendSettings(extractorParameters)
                                        .Build<BackendTemplateResources>();

            var backends = await this.backendClient.GetAllAsync(extractorParameters);
            if (backends.IsNullOrEmpty())
            {
                this.logger.LogWarning("No backends found for apim instance '{0}'", extractorParameters.SourceApimName);
                return backendTemplate;
            }

            foreach (var backendResource in backends)
            {
                backendResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{backendResource.OriginalName}')]";
                backendResource.Type = ResourceTypeConstants.Backend;
                backendResource.ApiVersion = GlobalConstants.ApiVersion;

                if (string.IsNullOrEmpty(singleApiName))
                {
                    // if the user is extracting all apis, extract all the backends
                    backendTemplate.TypedResources.Backends.Add(backendResource);
                    SaveBackendApiParametersToCache();
                }
                else
                {
                    if (apiPolicies.IsNullOrEmpty())
                    {
                        continue;
                    }

                    foreach (var policyTemplateResource in apiPolicies)
                    {
                        var policyContent = this.policyExtractor.GetCachedPolicyContent(policyTemplateResource, baseFilesGenerationDirectory);

                        if (this.DoesPolicyReferenceBackend(policyContent, backendResource.OriginalName, backendResource))
                        {
                            // backend was used in policy, extract it
                            backendTemplate.TypedResources.Backends.Add(backendResource);
                            SaveBackendApiParametersToCache();

                            // don't need to go through all policies if the back end has already been found
                            break;
                        }
                    }
                }

                void SaveBackendApiParametersToCache()
                {
                    if (!extractorParameters.ParameterizeBackend)
                    {
                        this.logger.LogDebug("Parameter '{0}' is false. Skipping storing api-backend mapping in cache", nameof(ExtractorParameters.ParameterizeBackend));
                        return;
                    }

                    var backendApiParameters = new BackendApiParameters();
                    var backendValidName = NamingHelper.GenerateValidParameterName(backendResource.OriginalName, ParameterPrefix.Backend).ToLower();

                    if (!string.IsNullOrEmpty(backendResource.Properties.ResourceId))
                    {
                        backendApiParameters.ResourceId = backendResource.Properties.ResourceId;
                        backendResource.Properties.ResourceId = $"[parameters('{ParameterNames.BackendSettings}').{backendValidName}.resourceId]";
                    }

                    if (!string.IsNullOrEmpty(backendResource.Properties.Url))
                    {
                        backendApiParameters.Url = backendResource.Properties.Url;
                        backendResource.Properties.Url = $"[parameters('{ParameterNames.BackendSettings}').{backendValidName}.url]";
                    }

                    if (!string.IsNullOrEmpty(backendResource.Properties.Protocol))
                    {
                        backendApiParameters.Protocol = backendResource.Properties.Protocol;
                        backendResource.Properties.Protocol = $"[parameters('{ParameterNames.BackendSettings}').{backendValidName}.protocol]";
                    }

                    if (!backendTemplate.TypedResources.BackendNameParametersCache.ContainsKey(backendValidName))
                    {
                        backendTemplate.TypedResources.BackendNameParametersCache.Add(backendValidName, backendApiParameters);
                    }

                    if (backendResource.Properties.Proxy != null)
                    {
                        var proxyUniqueId = $"{backendResource.Properties.Proxy.Url}_{backendResource.Properties.Proxy.Username}";
                        var backendProxyParameterName = NamingHelper.GenerateValidParameterName(proxyUniqueId, ParameterPrefix.BackendProxy);
                        
                        if (!backendTemplate.TypedResources.BackendProxyParametersCache.ContainsKey(backendProxyParameterName))
                        {
                            var backendProxyParameters = new BackendProxyParameters
                            {
                                Url = backendResource.Properties.Proxy.Url,
                                Username = backendResource.Properties.Proxy.Username,
                                Password = backendResource.Properties.Proxy.Password
                            };
                            backendTemplate.TypedResources.BackendProxyParametersCache.Add(backendProxyParameterName, backendProxyParameters);
                        }

                        backendResource.Properties.Proxy.Url = $"[parameters('{ParameterNames.BackendProxy}').{backendProxyParameterName}.url]";
                        backendResource.Properties.Proxy.Username = $"[parameters('{ParameterNames.BackendProxy}').{backendProxyParameterName}.username]";
                        backendResource.Properties.Proxy.Password = $"[parameters('{ParameterNames.BackendProxy}').{backendProxyParameterName}.password]";
                    }
                }
            }

            return backendTemplate;
        }

        bool DoesPolicyReferenceBackend(
            string policyContent,
            string backendName,
            BackendTemplateResource backendTemplateResource)
        {
            if (!string.IsNullOrEmpty(backendName) && policyContent.Contains(backendName) ||
                !string.IsNullOrEmpty(backendTemplateResource.Properties.Url) && policyContent.Contains(backendTemplateResource.Properties.Url) ||
                !string.IsNullOrEmpty(backendTemplateResource.Properties.Title) && policyContent.Contains(backendTemplateResource.Properties.Title) ||
                !string.IsNullOrEmpty(backendTemplateResource.Properties.ResourceId) && policyContent.Contains(backendTemplateResource.Properties.ResourceId))
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsNamedValueUsedInBackends(
            string singleApiName,
            List<PolicyTemplateResource> apiPolicies,
            string propertyName,
            string propertyDisplayName,
            ExtractorParameters extractorParameters,
            string baseFilesGenerationDirectory)
        {
            var backends = await this.backendClient.GetAllAsync(extractorParameters);
            if (backends.IsNullOrEmpty())
            {
                this.logger.LogWarning("No backends found for apim instance '{0}'", extractorParameters.SourceApimName);
                return false;
            }

            foreach (var backendResource in backends)
            {
                var backendContent = backendResource.Serialize();

                // check if backend references the named value, credentials for example
                if (backendContent.Contains(string.Concat("{{", propertyName, "}}")) || 
                    backendContent.Contains(string.Concat("{{", propertyDisplayName, "}}")))
                {
                    //only true if this is a full extraction, or in the case of a single api, if it is referenced by one of the API policies
                    if (string.IsNullOrEmpty(singleApiName))
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
                        var backendName = backendResource.Name;

                        // we have already checked if the named value is used in a policy, we just need to confirm if the backend is referenced by this single api within the policy file
                        // this is why an empty named values must be passed to this method for validation
                        foreach (var policyTemplateResource in apiPolicies)
                        {
                            var policyContent = this.policyExtractor.GetCachedPolicyContent(policyTemplateResource, baseFilesGenerationDirectory);
                            if (this.DoesPolicyReferenceBackend(policyContent, backendName, backendResource))
                            {
                                // don't need to go through all policies and backends if the named values has already been found
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
