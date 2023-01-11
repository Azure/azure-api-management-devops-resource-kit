// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
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
        readonly HttpClient httpClient;

        protected string BaseUrl { get; private set; } = GlobalConstants.BaseManagementAzureUrl;

        protected virtual AzureCliAuthenticator Auth { get; private set; } = new AzureCliAuthenticator();

        public ApiClientBase(IHttpClientFactory httpClientFactory)
        {
            this.httpClient = httpClientFactory.CreateClient();
        }

        protected async Task<(string,bool)> CallApiManagementAsync(string azToken, string requestUrl, bool useCache = true, ClientHttpMethod method = ClientHttpMethod.GET, bool catchMethodNotAllowed = false)
        {
            if (useCache && this.cache.TryGetValue(requestUrl, out string cachedResponseBody))
            {
                return (cachedResponseBody, false);
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
            string responseBody = await response.Content.ReadAsStringAsync();

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch(HttpRequestException ex)
            {
                if (!catchMethodNotAllowed)
                {
                    throw;
                }
                if (ex.StatusCode != HttpStatusCode.BadRequest)
                {
                    throw;
                }

                var errorData = responseBody.Deserialize<ApiErrorResponse>();
                if (errorData != null)
                {
                    if (errorData.Error.Code == ApiErrorCodes.MethodNotAllowed)
                    {
                        return (null, true);
                    }
                }

                throw;
            }
            
            if (useCache)
            {
                this.cache.Set(requestUrl, responseBody);
            }

            return (responseBody, false);
        }

        protected async Task<TResponse> GetResponseAsync<TResponse>(string azToken, string requestUrl, bool useCache = true, ClientHttpMethod method = ClientHttpMethod.GET)
        {
            var (stringResponse, _) = await this.CallApiManagementAsync(azToken, requestUrl, useCache, method);
            return stringResponse.Deserialize<TResponse>();
        }

        protected async Task<List<TResponse>> GetPagedResponseAsync<TResponse>(string azToken, string requestUrl, bool catchMethodNotAllowed = false)
        {
            var pageResponse = await MakePagedRequestAsync(requestUrl, catchMethodNotAllowed);

            var responseItems = new List<TResponse>(pageResponse.Items);
            while (pageResponse?.NextLink is not null)
            {
                pageResponse = await MakePagedRequestAsync(pageResponse.NextLink);
                responseItems.AddRange(pageResponse.Items);
            }

            return responseItems;

            async Task<AzurePagedResponse<TResponse>> MakePagedRequestAsync(string requestUrl, bool catchMethodNotAllowed = false)
            {
                var (stringResponse, returnDefault) = await this.CallApiManagementAsync(azToken, requestUrl, catchMethodNotAllowed: catchMethodNotAllowed);
                if (returnDefault)
                {
                    return this.GetEmptyPagedResponseAsync<TResponse>();
                }
                return stringResponse.Deserialize<AzurePagedResponse<TResponse>>();
            }
        }

        AzurePagedResponse<TResponse> GetEmptyPagedResponseAsync<TResponse>()
        {
            return new AzurePagedResponse<TResponse>()
            {
                Count = 0,
                Items = new List<TResponse>()
            };
        }
    }
}
