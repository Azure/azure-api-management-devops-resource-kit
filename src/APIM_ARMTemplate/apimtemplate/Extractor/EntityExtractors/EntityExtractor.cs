using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class EntityExtractor
    {
        public string baseUrl = "https://management.azure.com";
        internal Authentication auth = new Authentication();

        public static async Task<string> CallApiManagementAsync(string azToken, string requestUrl)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", azToken);

                HttpResponseMessage response = await httpClient.SendAsync(request);

                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
        }

        public Template GenerateEmptyTemplate()
        {
            TemplateCreator templateCreator = new TemplateCreator();
            Template armTemplate = templateCreator.CreateEmptyTemplate();
            return armTemplate;
        }

        public Template GenerateEmptyTemplateWithParameters(string policyXMLBaseUrl)
        {
            Template armTemplate = GenerateEmptyTemplate();
            armTemplate.parameters = new Dictionary<string, TemplateParameterProperties> { { "ApimServiceName", new TemplateParameterProperties() { type = "string" } } };
            if (policyXMLBaseUrl != null)
            {
                TemplateParameterProperties policyTemplateBaseUrlParameterProperties = new TemplateParameterProperties()
                {
                    type = "string"
                };
                armTemplate.parameters.Add("PolicyXMLBaseUrl", policyTemplateBaseUrlParameterProperties);
            }
            return armTemplate;
        }
    }
}
