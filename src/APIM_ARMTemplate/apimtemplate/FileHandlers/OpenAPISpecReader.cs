using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class OpenAPISpecReader
    {
        public OpenApiDocument ConvertToOpenAPISpec(string json)
        {
            OpenApiStringReader reader = new OpenApiStringReader();
            OpenApiDocument doc = reader.Read(json, out var diagnostic);
            return doc;
        }

        public OpenApiDocument ConvertLocalFileToOpenAPISpec(string jsonFileLocation)
        {
            JObject jObject = JObject.Parse(File.ReadAllText(jsonFileLocation));
            string json = JsonConvert.SerializeObject(jObject);
            OpenApiDocument document = ConvertToOpenAPISpec(json);
            return document;
        }

        public async Task<OpenApiDocument> ConvertRemoteURLToOpenAPISpecAsync(string remoteURL)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(remoteURL);
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                OpenApiDocument document = ConvertToOpenAPISpec(json);
                return document;
            }
            else
            {
                return new OpenApiDocument();
            }
        }
    }
}
