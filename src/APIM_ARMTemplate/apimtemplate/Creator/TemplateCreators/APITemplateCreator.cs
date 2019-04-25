using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.OpenApi.Models;
using System;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class APITemplateCreator
    {
        private FileReader fileReader;
        private TemplateCreator templateCreator;
        private PolicyTemplateCreator policyTemplateCreator;
        private ProductAPITemplateCreator productAPITemplateCreator;
        private DiagnosticTemplateCreator diagnosticTemplateCreator;

        public APITemplateCreator(FileReader fileReader, TemplateCreator templateCreator, PolicyTemplateCreator policyTemplateCreator, ProductAPITemplateCreator productAPITemplateCreator, DiagnosticTemplateCreator diagnosticTemplateCreator)
        {
            this.fileReader = fileReader;
            this.templateCreator = templateCreator;
            this.policyTemplateCreator = policyTemplateCreator;
            this.productAPITemplateCreator = productAPITemplateCreator;
            this.diagnosticTemplateCreator = diagnosticTemplateCreator;
        }

        public async Task<Template> CreateInitialAPITemplateAsync(CreatorConfig creatorConfig, APIConfig api)
        {
            // create empty template
            Template apiTemplate = this.templateCreator.CreateEmptyTemplate();

            // add parameters
            apiTemplate.parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { "ApimServiceName", new TemplateParameterProperties(){ type = "string" } }
            };

            List<TemplateResource> resources = new List<TemplateResource>();
            // create api resource w/ metadata
            APITemplateResource initialAPITemplateResource = await this.CreateInitialAPITemplateResourceAsync(creatorConfig, api);
            resources.Add(initialAPITemplateResource);

            apiTemplate.resources = resources.ToArray();
            return apiTemplate;
        }

        public Template CreateSubsequentAPITemplate(APIConfig api)
        {
            // create empty template
            Template apiTemplate = this.templateCreator.CreateEmptyTemplate();

            // add parameters
            apiTemplate.parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { "ApimServiceName", new TemplateParameterProperties(){ type = "string" } }
            };

            string apiName = api.name;
            string[] dependsOnSubsequentAPI = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{apiName}')]" };

            List<TemplateResource> resources = new List<TemplateResource>();
            // create api resource w/ swagger content and policies
            APITemplateResource subsequentAPITemplateResource = this.CreateSubsequentAPITemplateResource(api);
            PolicyTemplateResource apiPolicyResource = api.policy != null ? this.policyTemplateCreator.CreateAPIPolicyTemplateResource(api, dependsOnSubsequentAPI) : null;
            List<PolicyTemplateResource> operationPolicyResources = api.operations != null ? this.policyTemplateCreator.CreateOperationPolicyTemplateResources(api, dependsOnSubsequentAPI) : null;
            List<ProductAPITemplateResource> productAPIResources = api.products != null ? this.productAPITemplateCreator.CreateProductAPITemplateResources(api, dependsOnSubsequentAPI) : null;
            DiagnosticTemplateResource diagnosticTemplateResource = api.diagnostic != null ? this.diagnosticTemplateCreator.CreateAPIDiagnosticTemplateResource(api, dependsOnSubsequentAPI) : null;
            resources.Add(subsequentAPITemplateResource);
            // add resources if not null
            if (apiPolicyResource != null) resources.Add(apiPolicyResource);
            if (operationPolicyResources != null) resources.AddRange(operationPolicyResources);
            if (productAPIResources != null) resources.AddRange(productAPIResources);
            if (diagnosticTemplateResource != null) resources.Add(diagnosticTemplateResource);

            apiTemplate.resources = resources.ToArray();
            return apiTemplate;
        }

        public async Task<APITemplateResource> CreateInitialAPITemplateResourceAsync(CreatorConfig creatorConfig, APIConfig api)
        {
            // protocols can be pulled by converting the OpenApiSpec into the OpenApiDocument class
            OpenAPISpecReader openAPISpecReader = new OpenAPISpecReader();
            OpenApiDocument doc = await openAPISpecReader.ConvertOpenAPISpecToDoc(api.openApiSpec);

            // create api resource with properties
            APITemplateResource apiTemplateResource = new APITemplateResource()
            {
                name = $"[concat(parameters('ApimServiceName'), '/{api.name}')]",
                type = ResourceTypeConstants.API,
                apiVersion = "2018-06-01-preview",
                properties = new APITemplateProperties()
                {
                    // supplied via optional arguments
                    apiVersion = api.apiVersion,
                    subscriptionRequired = api.subscriptionRequired,
                    apiRevision = api.revision,
                    apiRevisionDescription = api.revisionDescription,
                    apiVersionDescription = api.apiVersionDescription,
                    authenticationSettings = api.authenticationSettings,
                    path = api.suffix,
                    displayName = api.name,
                    protocols = this.CreateProtocols(doc)
                },
                // if the template is not linked the depends on for the apiVersionSet needs to be inlined here
                dependsOn = new string[] { }
            };
            // set the version set id
            if (api.apiVersionSetId != null)
            {
                apiTemplateResource.properties.apiVersionSetId = $"[resourceId('Microsoft.ApiManagement/service/api-version-sets', parameters('ApimServiceName'), '{api.apiVersionSetId}')]";
            }
            return apiTemplateResource;
        }

        public APITemplateResource CreateSubsequentAPITemplateResource(APIConfig api)
        {
            // create api resource with properties
            string subsequentAPIName = $"[concat(parameters('ApimServiceName'), '/{api.name}')]";
            string subsequentAPIType = ResourceTypeConstants.API;
            Uri uriResult;
            bool isUrl = Uri.TryCreate(api.openApiSpec, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            // used to escape sequences in local swagger json
            object deserializedFileContents = isUrl ? null : JsonConvert.DeserializeObject<object>(this.fileReader.RetrieveLocalFileContents(api.openApiSpec));
            APITemplateResource apiTemplateResource = new APITemplateResource()
            {
                name = subsequentAPIName,
                type = subsequentAPIType,
                apiVersion = "2018-06-01-preview",
                properties = new APITemplateProperties()
                {
                    contentFormat = isUrl ? "swagger-link-json" : "swagger-json",
                    contentValue = isUrl ? api.openApiSpec : JsonConvert.SerializeObject(deserializedFileContents),
                    // supplied via optional arguments
                    path = api.suffix
                },
                dependsOn = new string[] { }
            };
            return apiTemplateResource;
        }

        public string[] CreateProtocols(OpenApiDocument doc)
        {
            // pull protocols from swagger OpenApiDocument
            List<string> protocols = new List<string>();
            foreach (OpenApiServer server in doc.Servers)
            {
                protocols.Add(server.Url.Split(":")[0]);
            }
            return protocols.ToArray();
        }
    }
}
