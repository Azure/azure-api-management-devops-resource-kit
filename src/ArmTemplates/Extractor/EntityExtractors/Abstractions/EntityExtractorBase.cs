using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions
{
    public abstract class EntityExtractorBase
    {
        public static string baseUrl = "https://management.azure.com";

        protected Authentication Auth = new Authentication();

        private static readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task<string> CallApiManagementAsync(string azToken, string requestUrl)
        {
            if (_cache.TryGetValue(requestUrl, out string cachedResponseBody))
            {
                return cachedResponseBody;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", azToken);

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            _cache.Set(requestUrl, responseBody);

            return responseBody;
        }

        public Template GenerateEmptyTemplate()
        {
            var templateCreator = new TemplateCreator();
            return templateCreator.CreateEmptyTemplate();
        }

        public Template GenerateEmptyPropertyTemplateWithParameters()
        {
            var armTemplate = GenerateEmptyTemplate();
            armTemplate.parameters = new Dictionary<string, TemplateParameterProperties>
            {
                {
                    ParameterNames.ApimServiceName, new TemplateParameterProperties() { type = "string" }
                }
            };

            return armTemplate;
        }
    }
}