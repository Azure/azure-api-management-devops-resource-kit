// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiOperations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.ApiOperations
{
    public class ApiOperationClient : ApiClientBase, IApiOperationClient
    {
        const string GetOperationsLinkedToApiRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations?api-version={5}";

        public async Task<List<ApiOperationTemplateResource>> GetOperationsLinkedToApiAsync(string apiName, ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(GetOperationsLinkedToApiRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, apiName, GlobalConstants.ApiVersion);

            return await this.GetPagedResponseAsync<ApiOperationTemplateResource>(azToken, requestUrl);
        }
    }
}
