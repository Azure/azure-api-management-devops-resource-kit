// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;
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
            List<NamedValueTemplateResource> namedValues,
            string baseFilesGenerationDirectory,
            ExtractorParameters extractorParameters)
        {
            var backendTemplate = this.templateBuilder
                                        .GenerateTemplateWithApimServiceNameProperty()
                                        .AddParameterizeBackendProperty(extractorParameters)
                                        .Build<BackendTemplateResources>();

            var backends = await this.backendClient.GetAllAsync(extractorParameters);
            if (backends.IsNullOrEmpty())
            {
                this.logger.LogWarning("No backends found for apim instance '{0}'", extractorParameters.SourceApimName);
                return backendTemplate;
            }

            foreach (var backendResource in backends)
            {
                var originalBackendName = backendResource.Name;

                backendResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{originalBackendName}')]";
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

                        if (this.DoesPolicyReferenceBackend(policyContent, namedValues, originalBackendName, backendResource))
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
                    var backendValidName = ParameterNamingHelper.GenerateValidParameterName(originalBackendName, ParameterPrefix.Diagnostic).ToLower();

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
                }
            }

            return backendTemplate;
        }

        bool DoesPolicyReferenceBackend(
            string policyContent,
            IEnumerable<NamedValueTemplateResource> namedValueResources,
            string backendName,
            BackendTemplateResource backendTemplateResource)
        {
            // a policy is referenced by a backend with the set-backend-service policy, which will reference use the backends name or url, or through referencing a named value that applies to the backend
            var namedValueResourcesUsedByBackend = namedValueResources.Where(resource => this.DoesBackendReferenceNamedValue(resource, backendTemplateResource));
            
            if (backendName != null && policyContent.Contains(backendName) ||
                backendTemplateResource.Properties.Url != null && policyContent.Contains(backendTemplateResource.Properties.Url) ||
                backendTemplateResource.Properties.Title != null && policyContent.Contains(backendTemplateResource.Properties.Title) ||
                backendTemplateResource.Properties.ResourceId != null && policyContent.Contains(backendTemplateResource.Properties.ResourceId))
            {
                return true;
            }

            return namedValueResourcesUsedByBackend.Any(x => 
                    (x.Properties.DisplayName is not null && policyContent.Contains(x.Properties.DisplayName)) ||
                    (x.Properties.Value is not null && policyContent.Contains(x.Properties.Value)));
        }

        public bool DoesBackendReferenceNamedValue(NamedValueTemplateResource namedValueResource, BackendTemplateResource backendTemplateResource)
        {
            var namedValue = namedValueResource.Properties.Value;
            
            return namedValue == backendTemplateResource.Properties.Url
                || namedValue == backendTemplateResource.Properties.Description
                || namedValue == backendTemplateResource.Properties.Title;
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
                            if (this.DoesPolicyReferenceBackend(policyContent, Array.Empty<NamedValueTemplateResource>(), backendName, backendResource))
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
