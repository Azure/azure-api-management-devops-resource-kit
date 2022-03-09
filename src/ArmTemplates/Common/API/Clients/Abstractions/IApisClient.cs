// --------------------------------------------------------------------------
//  <copyright file="IServiceApiClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions
{
    public interface IApisClient
    {
        Task<ApiTemplateResource> GetSingleAsync(ExtractorParameters extractorParameters, string apiName);

        Task<List<ApiTemplateResource>> GetAllAsync(ExtractorParameters extractorParameters);

        Task<List<ApiTemplateResource>> GetAllLinkedToProductAsync(ExtractorParameters extractorParameters, string productName);
    }
}
