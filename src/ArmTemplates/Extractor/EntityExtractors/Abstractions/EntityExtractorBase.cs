using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions
{
    // TODO delete this whole class. Extractors are not responsible for sending API requests - API Clients do it.
    // Extractors are getting api client in constructor and use their exposed API.
    // After getting the response, extractors are processing and generating templates.
    // This whole stuff will be done in https://github.com/Azure/azure-api-management-devops-resource-kit/issues/617 or related issues.

    // TODO also move TemplateGeneratorBase inheritance directly to Extractors implementation
    public abstract class EntityExtractorBase : TemplateGeneratorBase
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