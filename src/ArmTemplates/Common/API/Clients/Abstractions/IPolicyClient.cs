// --------------------------------------------------------------------------
//  <copyright file="IPolicyApiClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
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
    }
}
