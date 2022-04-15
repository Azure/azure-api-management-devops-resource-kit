// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions
{
    public interface IPolicyClient
    {
        Task<PolicyTemplateResource> GetGlobalServicePolicyAsync(ExtractorParameters extractorParameters);

        Task<PolicyTemplateResource> GetPolicyLinkedToProductAsync(string productName, ExtractorParameters extractorParameters);

        Task<PolicyTemplateResource> GetPolicyLinkedToApiAsync(string apiName, ExtractorParameters extractorParameters);

        Task<PolicyTemplateResource> GetPolicyLinkedToApiOperationAsync(string apiName, string operationName, ExtractorParameters extractorParameters);
    }
}
