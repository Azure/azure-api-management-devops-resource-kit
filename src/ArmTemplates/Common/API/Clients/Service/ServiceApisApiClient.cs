// --------------------------------------------------------------------------
//  <copyright file="ManagementInstanceApiClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Service.Responses;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Service;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Service
{
    public class ServiceApisApiClient : ApiClientBase, IServiceApisApiClient
    {
        public async Task<ServiceApiTemplateResource> GetSingleServiceApiAsync(string apiManagementName, string resourceGroupName, string apiName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(
                "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}?api-version={5}",
                this.BaseUrl, azSubId, resourceGroupName, apiManagementName, apiName, GlobalConstants.ApiVersion);

            return await this.CallApiManagementAsync<ServiceApiTemplateResource>(azToken, requestUrl);
        }

        public async Task<List<ServiceApiTemplateResource>> GetAllServiceApisAsync(ExtractorParameters extractorParameters)
        {
            return await this.GetAllServiceApisAsync(extractorParameters.SourceApimName, extractorParameters.ResourceGroup);
        }

        public async Task<List<ServiceApiTemplateResource>> GetAllServiceApisAsync(string apimInstanceName, string resourceGroupName)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();
            string requestUrl = string.Format(
                "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis?api-version={4}",
                this.BaseUrl, azSubId, resourceGroupName, apimInstanceName, GlobalConstants.ApiVersion);

            var response = await this.CallApiManagementAsync<GetAllServiceApisResponse>(azToken, requestUrl);
            return response.ServiceApis;
        }
    }
}
