// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers
{
    public class OpenAPISpecReader
    {
        public async Task<bool> IsJSONOpenAPISpecVersionThreeAsync(string openApiSpecFileLocation)
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
                    OpenAPISpecWithVersion openAPISpecWithVersion = fileContents.Deserialize<OpenAPISpecWithVersion>();
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
                OpenAPISpecWithVersion openAPISpecWithVersion = fileContents.Deserialize<OpenAPISpecWithVersion>();
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