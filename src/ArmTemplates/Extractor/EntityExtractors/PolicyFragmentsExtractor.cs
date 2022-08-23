// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.PolicyFragments;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class PolicyFragmentsExtractor : IPolicyFragmentsExtractor
    {
        readonly ILogger<PolicyFragmentsExtractor> logger;
        readonly IPolicyFragmentsClient policyFragmentsClient;
        readonly ITemplateBuilder templateBuilder;
        readonly IPolicyExtractor policyExtractor;

        public PolicyFragmentsExtractor(
            ILogger<PolicyFragmentsExtractor> logger,
            ITemplateBuilder templateBuilder,
            IPolicyFragmentsClient policyFragmentsClient,
            IPolicyExtractor policyExtractor)
        {
            this.logger = logger;
            this.templateBuilder = templateBuilder;
            this.policyFragmentsClient = policyFragmentsClient;
            this.policyExtractor = policyExtractor;
        }

        public async Task<Template<PolicyFragmentsResources>> GeneratePolicyFragmentsTemplateAsync(List<PolicyTemplateResource> apiTemplatePolicies, ExtractorParameters extractorParameters, string baseFilesGenerationDirectory)
        {
            var policyFragmentsTemplate = this.templateBuilder
                                        .GenerateTemplateWithApimServiceNameProperty()
                                        .Build<PolicyFragmentsResources>();

            var policyFragments = await this.policyFragmentsClient.GetAllAsync(extractorParameters);
            if (policyFragments.IsNullOrEmpty())
            {
                this.logger.LogWarning($"No policy fragments were found for '{extractorParameters.SourceApimName}' at '{extractorParameters.ResourceGroup}'");
                return policyFragmentsTemplate;
            }

            foreach (var policyFragment in policyFragments)
            {
                policyFragment.Type = ResourceTypeConstants.PolicyFragments;
                policyFragment.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{policyFragment.Name}')]";
                policyFragment.ApiVersion = GlobalConstants.ApiVersionPreview;

                if (string.IsNullOrEmpty(extractorParameters.SingleApiName))
                {
                    policyFragmentsTemplate.TypedResources.PolicyFragments.Add(policyFragment);
                }
                else
                {
                    var policyFragmentReferenceString = $"fragment-id=\"{policyFragment.OriginalName}\"";
                    var isReferencedInPolicy = this.DoesPolicyReferencePolicyFragment(apiTemplatePolicies, policyFragmentReferenceString, baseFilesGenerationDirectory);
                    
                    if (isReferencedInPolicy)
                    {
                        policyFragmentsTemplate.TypedResources.PolicyFragments.Add(policyFragment);
                    }
                }
            }

            return policyFragmentsTemplate;
        }

        bool DoesPolicyReferencePolicyFragment(
            List<PolicyTemplateResource> apiPolicies,
            string policyFragmentReferenceString,
            string baseFilesGenerationDirectory)
        {
            if (apiPolicies.IsNullOrEmpty())
            {
                return false;
            }

            foreach (var policyTemplateResource in apiPolicies)
            {
                var policyContent = this.policyExtractor.GetCachedPolicyContent(policyTemplateResource, baseFilesGenerationDirectory);

                if (policyContent.Contains(policyFragmentReferenceString))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
