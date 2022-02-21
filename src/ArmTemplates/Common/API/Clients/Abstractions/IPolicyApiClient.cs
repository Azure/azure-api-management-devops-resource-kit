// --------------------------------------------------------------------------
//  <copyright file="IPolicyApiClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions
{
    public interface IPolicyApiClient
    {
        Task<PolicyTemplateResource> GetGlobalServicePolicyAsync(string apiManagementName, string resourceGroupName);
    }
}
