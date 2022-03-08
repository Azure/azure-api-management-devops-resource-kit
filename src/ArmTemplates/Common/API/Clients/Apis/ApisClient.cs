// --------------------------------------------------------------------------
//  <copyright file="ManagementInstanceApiClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Apis.Responses;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Service;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Apis
{
    public class ApisClient : ApiClientBase, IApisClient
    {
        const string GetSingleApiRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}?api-version={5}";
        const string GetAllApisRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis?api-version={4}";

        public async Task<ApiTemplateResource> GetSingleAsync(string apiManagementName, string resourceGroupName, string apiName)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();
            string requestUrl = string.Format(GetSingleApiRequest,
                this.BaseUrl, azSubId, resourceGroupName, apiManagementName, apiName, GlobalConstants.ApiVersion);

            return await this.CallApiManagementAsync<ApiTemplateResource>(azToken, requestUrl);
        }

        public async Task<List<ApiTemplateResource>> GetAllAsync(ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();
            string requestUrl = string.Format(GetAllApisRequest,
                this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, GlobalConstants.ApiVersion);

            var response = await this.CallApiManagementAsync<GetAllApisResponse>(azToken, requestUrl);
            return response.Apis;
        }
    }
}
