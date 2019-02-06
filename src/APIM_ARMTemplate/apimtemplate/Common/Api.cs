using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
        public async Task<string> GetAPIOperationDetail(string ApiManagementName, string ResourceGroupName, string ApiName, string OperationName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations/{5}?api-version={6}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, OperationName, Constants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }
        public async Task<string> GetOperationPolicy(string ApiManagementName, string ResourceGroupName, string ApiName, string OperationId)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations/{5}/policies?api-version={6}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, OperationId, Constants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }
        public async Task<string> GetAPIDetails(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, Constants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }
        public async Task<string> GetAPIVersionSet(string ApiManagementName, string ResourceGroupName, string VersionSetName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();
                                                                                                                         
            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/{4}?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, VersionSetName, Constants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }
        public async Task<string> GetAPIs(string ApiManagementName, string ResourceGroupName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis?api-version={4}",
                baseUrl, azSubId, ResourceGroupName, ApiManagementName, Constants.APIVersion);
          
            return await CallApiManagement(azToken, requestUrl);
        }
        public async Task<string> GetAPIPolicies(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();
        
            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/policies/policy?api-version={5}",
                baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, Constants.APIVersion);

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