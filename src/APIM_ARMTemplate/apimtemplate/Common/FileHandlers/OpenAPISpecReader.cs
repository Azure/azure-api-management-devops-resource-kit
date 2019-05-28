using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    public class OpenAPISpecReader
    {
        public async Task<bool> isJSONOpenAPISpecVersionThreeAsync(string openApiSpecFileLocation)
        {
            // determine whether file location is local file path or remote url and read content
            Uri uriResult;
            bool isUrl = Uri.TryCreate(openApiSpecFileLocation, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (isUrl)
            {

                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(uriResult);
                if (response.IsSuccessStatusCode)
                {
                    string fileContents = await response.Content.ReadAsStringAsync();
                    OpenAPISpecWithVersion openAPISpecWithVersion = JsonConvert.DeserializeObject<OpenAPISpecWithVersion>(fileContents);
                    // OASv3 has the property 'openapi' but not the property 'swagger'
                    return openAPISpecWithVersion.Swagger != null ? false : true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                string fileContents = File.ReadAllText(openApiSpecFileLocation);
                OpenAPISpecWithVersion openAPISpecWithVersion = JsonConvert.DeserializeObject<OpenAPISpecWithVersion>(fileContents);
                // OASv3 has the property 'openapi' but not the property 'swagger'
                return openAPISpecWithVersion.Swagger != null ? false : true;
            }
        }
    }

    public class OpenAPISpecWithVersion
    {
        // OASv3 has the property 'swagger'
        [JsonProperty(PropertyName = "swagger")]
        public string Swagger { get; set; }
        // OASv3 has the property 'openapi'
        [JsonProperty(PropertyName = "openapi")]
        public string OpenAPISpec { get; set; }
    }

}