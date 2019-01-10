using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    class Api
    {
        static string baseUrl = "https://management.azure.com";
        internal Authentication auth = new Authentication();
        public async Task<string> GetAPIOperations(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, Constants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);

        }
        public async Task<string> GetAPIs(string ApiManagementName, string ResourceGroupName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis?api-version={4}",
                baseUrl, azSubId, ResourceGroupName, ApiManagementName, Constants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);

        }
        private static async Task<string> CallApiManagement(string azToken, string requestUrl)
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
    }
}
