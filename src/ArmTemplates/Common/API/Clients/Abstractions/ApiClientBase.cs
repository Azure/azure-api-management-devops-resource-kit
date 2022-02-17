// --------------------------------------------------------------------------
//  <copyright file="ApiClientBase.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions
{
    public abstract class ApiClientBase
    {
        public static string BaseUrl = "https://management.azure.com";

        protected Authentication Auth = new Authentication();

        static readonly IMemoryCache Cache = new MemoryCache(new MemoryCacheOptions());
        static readonly HttpClient HttpClient = new HttpClient();

        protected async Task<string> CallApiManagementAsync(string azToken, string requestUrl)
        {
            if (Cache.TryGetValue(requestUrl, out string cachedResponseBody))
            {
                return cachedResponseBody;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", azToken);

            HttpResponseMessage response = await HttpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            Cache.Set(requestUrl, responseBody);

            return responseBody;
        }

        protected async Task<TResponse> CallApiManagementAsync<TResponse>(string azToken, string requestUrl)
        {
            var stringResponse = await this.CallApiManagementAsync(azToken, requestUrl);
            return stringResponse.Deserialize<TResponse>();
        }
    }
}
