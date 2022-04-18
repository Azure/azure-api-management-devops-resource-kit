// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions
{
    public interface IApisClient
    {
        Task<ApiTemplateResource> GetSingleAsync(string apiName, ExtractorParameters extractorParameters);

        Task<List<ApiTemplateResource>> GetAllAsync(ExtractorParameters extractorParameters);

        Task<List<ApiTemplateResource>> GetAllLinkedToProductAsync(string productName, ExtractorParameters extractorParameters);

        Task<List<ApiTemplateResource>> GetAllLinkedToGatewayAsync(string gatewayName, ExtractorParameters extractorParameters);
    }
}
