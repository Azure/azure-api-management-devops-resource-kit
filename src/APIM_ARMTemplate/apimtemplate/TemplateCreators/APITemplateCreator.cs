using System.IO;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class APITemplateCreator
    {
        public async Task<APITemplate> CreateAPITemplateAsync(OpenApiDocument doc, CLICreatorArguments cliArguments)
        {
            YAMLReader yamlReader = new YAMLReader();
            // create api schema with properties
            APITemplate apiSchema = new APITemplate()
            {
                type = "Microsoft.ApiManagement/service/apis",
                apiVersion = "2018-06-01-preview",
                properties = new APITemplateProperties()
                {
                    contentFormat = "swagger-json",
                    contentValue = await CreateOpenAPISpecContentsAsync(cliArguments),
                    // supplied via optional arguments
                    apiVersion = cliArguments.apiVersion ?? "",
                    apiRevision = cliArguments.apiRevision ?? "",
                    apiVersionSetId = cliArguments.apiVersionSetId ?? "",
                    path = cliArguments.path ?? "",
                    apiRevisionDescription = cliArguments.apiRevisionDescription ?? "",
                    apiVersionDescription = cliArguments.apiVersionDescription ?? "",
                    apiVersionSet = cliArguments.apiVersionSetFile != null ? yamlReader.ConvertYAMLFileToAPIVersionSet(cliArguments.apiVersionSetFile) : null,
                    authenticationSettings = cliArguments.authenticationSettingsFile != null ? yamlReader.ConvertYAMLFileToAuthenticationSettings(cliArguments.authenticationSettingsFile) : null
                }
            };
            return apiSchema;
        }

        public async Task<string> CreateOpenAPISpecContentsAsync(CLICreatorArguments cliArguments)
        {
            // return contents of supplied Open API Spec file
            if (cliArguments.openAPISpecFile != null)
            {
                return File.ReadAllText(cliArguments.openAPISpecFile);
            }
            else if (cliArguments.openAPISpecURL != null)
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(cliArguments.openAPISpecURL);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    return json;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
    }
}
