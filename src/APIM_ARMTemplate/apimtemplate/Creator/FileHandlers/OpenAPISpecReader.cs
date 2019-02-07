using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class OpenAPISpecReader
    {
        public OpenApiDocument ConvertToOpenAPISpec(string json)
        {
            // converts json string into OpenApiDocument class
            OpenApiStringReader reader = new OpenApiStringReader();
            OpenApiDocument doc = reader.Read(json, out var diagnostic);
            return doc;
        }

        public OpenApiDocument ConvertLocalFileToOpenAPISpec(string jsonFile)
        {
            JObject jObject = JObject.Parse(File.ReadAllText(jsonFile));
            string json = JsonConvert.SerializeObject(jObject);
            OpenApiDocument document = ConvertToOpenAPISpec(json);
            return document;
        }

        public async Task<OpenApiDocument> ConvertRemoteURLToOpenAPISpecAsync(Uri uriResult)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(uriResult);
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

        public async Task<OpenApiDocument> ConvertOpenAPISpecToDoc(string openApiSpecFileLocation)
        {
            // determine whether file location is local file path or remote url and convert appropriately
            Uri uriResult;
            bool isUrl = Uri.TryCreate(openApiSpecFileLocation, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (isUrl)
            {
                return await this.ConvertRemoteURLToOpenAPISpecAsync(uriResult);
            }
            else
            {
                return this.ConvertLocalFileToOpenAPISpec(openApiSpecFileLocation);
            }
        }
    }
}
