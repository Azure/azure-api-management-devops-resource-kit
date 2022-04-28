// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions
{
    public abstract class ApiClientBase
    {
        readonly IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
        readonly HttpClient httpClient = new HttpClient();

        protected string BaseUrl { get; private set; } = GlobalConstants.BaseManagementAzureUrl;

        protected AzureCliAuthenticator Auth { get; private set; } = new AzureCliAuthenticator();

        public ApiClientBase(string baseUrl = null) 
        {
            if (!string.IsNullOrEmpty(baseUrl))
            {
                this.BaseUrl = baseUrl;
            }
        }

        protected async Task<string> CallApiManagementAsync(string azToken, string requestUrl)
        {
            if (this.cache.TryGetValue(requestUrl, out string cachedResponseBody))
            {
                return cachedResponseBody;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", azToken);

            HttpResponseMessage response = await this.httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            this.cache.Set(requestUrl, responseBody);

            return responseBody;
        }

        protected async Task<TResponse> GetResponseAsync<TResponse>(string azToken, string requestUrl)
        {
            var stringResponse = await this.CallApiManagementAsync(azToken, requestUrl);
            return stringResponse.Deserialize<TResponse>();
        }

        protected async Task<List<TResponse>> GetPagedResponseAsync<TResponse>(string azToken, string requestUrl)
        {
            var pageResponse = await MakePagedRequestAsync(requestUrl);

            var responseItems = new List<TResponse>(pageResponse.Items);
            while (pageResponse?.NextLink is not null)
            {
                pageResponse = await MakePagedRequestAsync(pageResponse.NextLink);
                responseItems.AddRange(pageResponse.Items);
            }

            return responseItems;

            async Task<AzurePagedResponse<TResponse>> MakePagedRequestAsync(string requestUrl)
            {
                var stringResponse = await this.CallApiManagementAsync(azToken, requestUrl);
                return stringResponse.Deserialize<AzurePagedResponse<TResponse>>();
            }
        }
    }
}
