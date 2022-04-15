// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Gateway;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions
{
    public interface IGatewayClient
    {
        Task<List<GatewayTemplateResource>> GetAllAsync(ExtractorParameters extractorParameters);

        Task<bool> DoesApiReferenceGatewayAsync(string singleApiName, string gatewayName, ExtractorParameters extractorParameters);
    }
}
