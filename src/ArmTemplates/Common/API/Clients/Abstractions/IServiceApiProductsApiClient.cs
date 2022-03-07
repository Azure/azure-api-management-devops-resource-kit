// --------------------------------------------------------------------------
//  <copyright file="IProductApisApiClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions
{
    public interface IServiceApiProductsApiClient
    {
        Task<List<ServiceApisProductTemplateResource>> GetServiceApiProductsAsync(string apiManagementName, string resourceGroupName, string apiName);
    }
}
