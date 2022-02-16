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
        public static string BaseUrl = "https://management.azure.com";

        protected Authentication Auth = new Authentication();

        static readonly IMemoryCache Cache = new MemoryCache(new MemoryCacheOptions());
        static readonly HttpClient HttpClient = new HttpClient();

        public async Task<string> CallApiManagementAsync(string azToken, string requestUrl)
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

        public Template GenerateEmptyTemplate()
        {
            var templateCreator = new TemplateCreator();
            return templateCreator.CreateEmptyTemplate();
        }

        public Template GenerateEmptyPropertyTemplateWithParameters()
        {
            var armTemplate = this.GenerateEmptyTemplate();
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