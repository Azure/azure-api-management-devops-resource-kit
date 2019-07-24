using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;

namespace apimtemplate.Creator.Extensions
{
    public class SpecificationCopy
    {
        private static HttpClient _client;
        public static async Task<string> CopyOpenApiSpecification(APIConfig api)
        {
            if (string.IsNullOrEmpty(api.openApiSpecCopyToLocation))
            {
                return api.openApiSpec;
            }

            if (string.IsNullOrEmpty(api.openApiSpec))
            {
                throw new ArgumentNullException(nameof(api.openApiSpec), "Missing API specification source path");
            }

            if (_client == null)
            {
                _client = new HttpClient();
            }

            Console.WriteLine("Downloading OpenApiSpec file:");
            Console.WriteLine($"uri = {api.openApiSpec} | sas = {api.sasToken} | destination = {api.openApiSpecCopyToLocation}");
            var url = string.IsNullOrEmpty(api.sasToken) ? api.openApiSpec : $"{api.openApiSpec}?{api.sasToken}";
            await DownloadAsync(url, api.openApiSpecCopyToLocation);
            return api.openApiSpecCopyToLocation;
        }

        private static async Task DownloadAsync(string uri, string destination)
        {
            Console.WriteLine($"Downloading OpenApiSpec file from : {uri}");
            var response = await _client.GetAsync(uri).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            if (response.IsSuccessStatusCode)
            {
                using (var fileWriter = new StreamWriter(new FileStream(destination, FileMode.Create)))
                {
                    await fileWriter.WriteAsync(content).ConfigureAwait(false);
                }
            }
            else
            {
                var msg = $"{response.StatusCode}: {response.Content.ReadAsStringAsync()}";
                Console.WriteLine(msg);
                throw new ArgumentException(msg);
            }
        }
    }
}
