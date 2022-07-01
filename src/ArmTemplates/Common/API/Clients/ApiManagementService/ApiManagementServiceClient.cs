// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiManagementService;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.ApiManagementService
{
    public class ApiManagementServiceClient: ApiClientBase, IApiManagementServiceClient
    {
        const string GetApiManagementServiceByName = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}?api-version={4}";

        readonly IApiManagementServiceProcessor apiManagementServiceProcessor;

        public ApiManagementServiceClient(
            IHttpClientFactory httpClientFactory, 
            IApiManagementServiceProcessor apiManagementServiceProcessor): base(httpClientFactory)
        {
            this.apiManagementServiceProcessor = apiManagementServiceProcessor;
        }

        public async Task<ApiManagementServiceResource> GetApiManagementService(ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(GetApiManagementServiceByName,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, GlobalConstants.ApiVersion);

            var apiManagementServiceResource = await this.GetResponseAsync<ApiManagementServiceResource>(azToken, requestUrl);
            this.apiManagementServiceProcessor.ProcessSingleInstanceData(apiManagementServiceResource, extractorParameters);

            return apiManagementServiceResource;
        }
    }
}