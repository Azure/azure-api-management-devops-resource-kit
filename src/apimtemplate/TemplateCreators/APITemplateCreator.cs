using System.IO;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class APITemplateCreator
    {
        public async Task<APITemplate> CreateAPITemplateAsync(OpenApiDocument doc, CLICreatorArguments cliArguments)
        {
            // create api schema with properties
            APITemplate apiSchema = new APITemplate()
            {
                name = doc.Info.Title,
                type = "Microsoft.ApiManagement/service/apis",
                apiVersion = "2018-06-01-preview",
                properties = new APITemplateProperties()
                {
                    contentFormat = "swagger-json",
                    contentValue = await CreateOpenAPISpecContentsAsync(cliArguments),
                    subscriptionRequired = IsSubscriptionRequired(doc),
                    protocols = CreateProtocols(doc),
                    serviceUrl = doc.Servers[0].Url,
                    subscriptionKeyParameterNames = CreateSubscriptionKeyParameterNames(doc),
                    description = doc.Info.Description,
                    displayName = doc.Info.Title,
                    apiVersion = cliArguments.apiVersion ?? doc.Info.Version,
                    apiRevision = cliArguments.apiRevision ?? "",
                    apiVersionSetId = cliArguments.apiVersionSetId ?? "",
                    path = cliArguments.path ?? "",
                    // assumptions
                    type = "http",
                    apiType = "http",
                    wsdlSelector = null,

                    // unfinished
                    authenticationSettings = CreateAuthenticationSettings(doc),
                    apiRevisionDescription = null,
                    apiVersionDescription = null,
                    apiVersionSet = null
                }
            };

            // create resources
            List<APITemplateResource> resources = new List<APITemplateResource>();
            SchemaTemplateCreator schemaCreator = new SchemaTemplateCreator();
            List<SchemaTemplate> schemaTemplates = schemaCreator.CreateSchemaTemplates(doc);
            OperationTemplateCreator operationCreator = new OperationTemplateCreator();
            List<OperationTemplate> operationTemplates = operationCreator.CreateOperationTemplates(doc);
            resources.AddRange(schemaTemplates);
            resources.AddRange(operationTemplates);
            apiSchema.resources = resources.ToArray();

            return apiSchema;
        }

        public APITemplateAuthenticationSettings CreateAuthenticationSettings(OpenApiDocument doc)
        {
            //unfinished
            APITemplateAuthenticationSettings authenticationSettings = new APITemplateAuthenticationSettings()
            {
                subscriptionKeyRequired = false
            };
            foreach (OpenApiSecurityScheme securityScheme in doc.Components.SecuritySchemes.Values)
            {
                if (securityScheme.Type == SecuritySchemeType.OAuth2)
                {
                    authenticationSettings.oAuth2 = new APITemplateOAuth2()
                    {
                        authorizationServerId = null,
                        scope = null
                    };
                }
                else if (securityScheme.Type == SecuritySchemeType.OpenIdConnect)
                {
                    authenticationSettings.openid = new APITemplateOpenID()
                    {
                        openidProviderId = null,
                        bearerTokenSendingMethods = new string[] { }
                    };
                }
            };
            return authenticationSettings;
        }

        public async Task<string> CreateOpenAPISpecContentsAsync(CLICreatorArguments cliArguments)
        {
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

        public APITemplateSubscriptionKeyParameterNames CreateSubscriptionKeyParameterNames(OpenApiDocument doc)
        {
            string header = "";
            string query = "";
            foreach (OpenApiSecurityRequirement requirement in doc.SecurityRequirements)
            {
                foreach (OpenApiSecurityScheme scheme in requirement.Keys)
                {
                    if (scheme.In == ParameterLocation.Query)
                    {
                        query = scheme.Name;
                    }
                    else if (scheme.In == ParameterLocation.Header)
                    {
                        header = scheme.Name;
                    }
                }
            }
            APITemplateSubscriptionKeyParameterNames subscriptionKeyParameterNames = new APITemplateSubscriptionKeyParameterNames()
            {
                header = header,
                query = query
            };
            return subscriptionKeyParameterNames;
        }

        public bool IsSubscriptionRequired(OpenApiDocument doc)
        {
            bool required = false;
            foreach (OpenApiSecurityRequirement requirement in doc.SecurityRequirements)
            {
                foreach (OpenApiSecurityScheme scheme in requirement.Keys)
                {
                    if (scheme.Name.ToLower().Contains("subscription-key"))
                    {
                        required = true;
                    }
                }
            }
            return required;
        }

        public string[] CreateProtocols(OpenApiDocument doc)
        {
            List<string> protocols = new List<string>();
            foreach (OpenApiServer server in doc.Servers)
            {
                protocols.Add(server.Url.Split(":")[0]);
            }
            return protocols.ToArray();
        }
    }
}
