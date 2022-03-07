// --------------------------------------------------------------------------
//  <copyright file="IServiceApiClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Service;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions
{
    public interface IServiceApisApiClient
    {
        Task<ServiceApiTemplateResource> GetSingleServiceApiAsync(string apiManagementName, string resourceGroupName, string apiName);

        Task<List<ServiceApiTemplateResource>> GetAllServiceApisAsync(ExtractorParameters extractorParameters);

        Task<List<ServiceApiTemplateResource>> GetAllServiceApisAsync(string apimInstanceName, string resourceGroupName);
    }
}
