using System.Collections.Generic;
using System.Net.Http;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class EntityExtractor
    {
        public static string baseUrl = "https://management.azure.com";
        internal Authentication auth = new Authentication();
        private static readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public static async Task<string> CallApiManagementAsync(string azToken, string requestUrl)
        {
            if (_cache.TryGetValue(requestUrl, out string cachedResponseBody))
            {
                return cachedResponseBody;
            }

            using (HttpClient httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", azToken);

                HttpResponseMessage response = await httpClient.SendAsync(request);

                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                _cache.Set(requestUrl, responseBody);

                return responseBody;
            }
        }

        public Template GenerateEmptyTemplate()
        {
            TemplateCreator templateCreator = new TemplateCreator();
            Template armTemplate = templateCreator.CreateEmptyTemplate();
            return armTemplate;
        }

        public Template GenerateEmptyPropertyTemplateWithParameters()
        {
            Template armTemplate = GenerateEmptyTemplate();
            armTemplate.parameters = new Dictionary<string, TemplateParameterProperties> { { ParameterNames.ApimServiceName, new TemplateParameterProperties() { type = "string" } } };
            return armTemplate;
        }


        

    }
}