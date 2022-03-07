// --------------------------------------------------------------------------
//  <copyright file="ProductApisApiClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.ProductApis.Responses;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.ProductApis
{
    public class ServiceApisProductsApiClient : ApiClientBase, IServiceApiProductsApiClient
    {
        public async Task<List<ServiceApisProductTemplateResource>> GetServiceApiProductsAsync(string apiManagementName, string resourceGroupName, string apiName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(
                "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/products?api-version={5}",
               this.BaseUrl, azSubId, resourceGroupName, apiManagementName, apiName, GlobalConstants.ApiVersion);

            var response = await this.CallApiManagementAsync<GetAllServiceApiProductsResponse>(azToken, requestUrl);
            return response.ServiceApisProducts;
        }
    }
}
