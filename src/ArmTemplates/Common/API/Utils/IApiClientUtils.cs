// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Utils
{
    public interface IApiClientUtils
    {
        Task<Dictionary<string, List<string>>> GetAllAPIsDictionaryByVersionSetName(ExtractorParameters extractorParameters);

        Task<ApiTemplateResource> GetsingleApi(string apiName, ExtractorParameters extractorParameters);

        Task<bool> DoesApiReferenceGatewayAsync(string singleApiName, string gatewayName, ExtractorParameters extractorParameters);
    }
}
