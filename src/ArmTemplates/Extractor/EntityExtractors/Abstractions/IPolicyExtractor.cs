// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions
{
    public interface IPolicyExtractor
    {
        string GetCachedPolicyContent(PolicyTemplateResource policyTemplateResource, string baseFilesGenerationDirectory);

        Task<PolicyTemplateResource> GenerateProductPolicyTemplateAsync(
            ExtractorParameters extractorParameters,
            string productName,
            string[] productResourceId,
            string baseFilesGenerationDirectory);

        Task<PolicyTemplateResource> GenerateApiPolicyResourceAsync(
            string apiName,
            string baseFilesGenerationDirectory,
            ExtractorParameters extractorParameters);

        Task<PolicyTemplateResource> GenerateApiOperationPolicyResourceAsync(
            string apiName,
            string operationName,
            string baseFilesGenerationDirectory,
            ExtractorParameters extractorParameters);

        Task<Template<PolicyTemplateResources>> GenerateGlobalServicePolicyTemplateAsync(ExtractorParameters extractorParameters, string baseFilesGenerationDirectory);
    }
}
