﻿// --------------------------------------------------------------------------
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

        protected virtual AzureCliAuthenticator Auth { get; private set; } = new AzureCliAuthenticator();

        public ApiClientBase(string baseUrl = null) 
        {
            if (!string.IsNullOrEmpty(baseUrl))
            {
                this.BaseUrl = baseUrl;
            }
        }

        protected virtual async Task<string> CallApiManagementAsync(string azToken, string requestUrl, bool useCache = true, ClientHttpMethod method = ClientHttpMethod.GET)
        {
            if (useCache && this.cache.TryGetValue(requestUrl, out string cachedResponseBody))
            {
                return cachedResponseBody;
            }

            var httpMethod = method switch
            {
                ClientHttpMethod.GET => HttpMethod.Get,
                ClientHttpMethod.POST => HttpMethod.Post,
                _ => throw new NotImplementedException("Method not supported")
            };

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

        protected async Task<TResponse> GetResponseAsync<TResponse>(string azToken, string requestUrl, bool useCache = true, ClientHttpMethod method = ClientHttpMethod.GET)
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
