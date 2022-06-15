// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
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

        protected Dictionary<string, HttpMethod> httpMethodsMap;

        public const string HTTP_GET_METHOD = "get";
        public const string HTTP_POST_METHOD = "post";

        public ApiClientBase(string baseUrl = null) 
        {
            if (!string.IsNullOrEmpty(baseUrl))
            {
                this.BaseUrl = baseUrl;
            }

            this.httpMethodsMap = new Dictionary<string, HttpMethod>();
            this.httpMethodsMap.Add(HTTP_GET_METHOD, HttpMethod.Get);
            this.httpMethodsMap.Add(HTTP_POST_METHOD, HttpMethod.Post);
        }

        protected async Task<string> CallApiManagementAsync(string azToken, string requestUrl, bool useCache = true, string method = HTTP_GET_METHOD)
        {
            if (useCache && this.cache.TryGetValue(requestUrl, out string cachedResponseBody))
            {
                return cachedResponseBody;
            }

            if (!this.httpMethodsMap.ContainsKey(method))
            {
                throw new Exception($"Method {method} is not defined");
            }

            var httpMethod = this.httpMethodsMap[method];
            var request = new HttpRequestMessage(httpMethod, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", azToken);
            request.Headers.UserAgent.TryParseAdd($"{Application.Name}/{Application.BuildVersion}");

            HttpResponseMessage response = await this.httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            if (useCache)
            {
                this.cache.Set(requestUrl, responseBody);
            }

            return responseBody;
        }

        protected async Task<TResponse> GetResponseAsync<TResponse>(string azToken, string requestUrl, bool useCache = true, string method = "get")
        {
            var stringResponse = await this.CallApiManagementAsync(azToken, requestUrl, useCache, method);
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
