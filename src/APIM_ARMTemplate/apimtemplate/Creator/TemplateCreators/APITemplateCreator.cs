using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.OpenApi.Models;
using System;

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

        public async Task<Template> CreateInitialAPITemplateAsync(CreatorConfig creatorConfig)
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
            APITemplateResource initialAPITemplateResource = await this.CreateInitialAPITemplateResourceAsync(creatorConfig);
            resources.Add(initialAPITemplateResource);

            apiTemplate.resources = resources.ToArray();
            return apiTemplate;
        }

        public Template CreateSubsequentAPITemplate(CreatorConfig creatorConfig)
        {
            // create empty template
            Template apiTemplate = this.templateCreator.CreateEmptyTemplate();

            // add parameters
            apiTemplate.parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { "ApimServiceName", new TemplateParameterProperties(){ type = "string" } }
            };

            string apiName = creatorConfig.api.name;
            string[] dependsOnSubsequentAPI = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{apiName}')]" };

            List<TemplateResource> resources = new List<TemplateResource>();
            // create api resource w/ swagger content and policies
            APITemplateResource subsequentAPITemplateResource = this.CreateSubsequentAPITemplateResource(creatorConfig);
            PolicyTemplateResource apiPolicyResource = creatorConfig.api.policy != null ? this.policyTemplateCreator.CreateAPIPolicyTemplateResource(creatorConfig, dependsOnSubsequentAPI) : null;
            List<PolicyTemplateResource> operationPolicyResources = creatorConfig.api.operations != null ? this.policyTemplateCreator.CreateOperationPolicyTemplateResources(creatorConfig, dependsOnSubsequentAPI) : null;
            List<ProductAPITemplateResource> productAPIResources = creatorConfig.api.products != null ? this.productAPITemplateCreator.CreateProductAPITemplateResources(creatorConfig, dependsOnSubsequentAPI) : null;
            DiagnosticTemplateResource diagnosticTemplateResource = creatorConfig.diagnostic != null ? this.diagnosticTemplateCreator.CreateAPIDiagnosticTemplateResource(creatorConfig, dependsOnSubsequentAPI) : null;
            resources.Add(subsequentAPITemplateResource);
            // add resources if not null
            if (apiPolicyResource != null) resources.Add(apiPolicyResource);
            if (operationPolicyResources != null) resources.AddRange(operationPolicyResources);
            if (productAPIResources != null) resources.AddRange(productAPIResources);
            if (diagnosticTemplateResource != null) resources.Add(diagnosticTemplateResource);

            apiTemplate.resources = resources.ToArray();
            return apiTemplate;
        }

        public async Task<APITemplateResource> CreateInitialAPITemplateResourceAsync(CreatorConfig creatorConfig)
        {
            // protocols can be pulled by converting the OpenApiSpec into the OpenApiDocument class
            OpenAPISpecReader openAPISpecReader = new OpenAPISpecReader();
            OpenApiDocument doc = await openAPISpecReader.ConvertOpenAPISpecToDoc(creatorConfig.api.openApiSpec);

            // create api resource with properties
            APITemplateResource apiTemplateResource = new APITemplateResource()
            {
                name = $"[concat(parameters('ApimServiceName'), '/{creatorConfig.api.name}')]",
                type = "Microsoft.ApiManagement/service/apis",
                apiVersion = "2018-01-01",
                properties = new APITemplateProperties()
                {
                    // supplied via optional arguments
                    apiVersion = creatorConfig.api.apiVersion,
                    apiRevision = creatorConfig.api.revision,
                    apiRevisionDescription = creatorConfig.api.revisionDescription,
                    apiVersionDescription = creatorConfig.api.apiVersionDescription,
                    authenticationSettings = creatorConfig.api.authenticationSettings,
                    path = creatorConfig.api.suffix,
                    displayName = creatorConfig.api.name,
                    protocols = this.CreateProtocols(doc)
                },
                // if the template is not linked the depends on for the apiVersionSet needs to be inlined here
                dependsOn = new string[] { }
            };
            string versionSetId = (creatorConfig.apiVersionSet != null && creatorConfig.apiVersionSet.id != null) ? creatorConfig.apiVersionSet.id : "versionset";
            // if the template is linked and a version set was created, the initial api depends on it
            if (creatorConfig.linked == false && creatorConfig.apiVersionSet != null)
            {
                apiTemplateResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/api-version-sets', parameters('ApimServiceName'), '{versionSetId}')]" };
            }
            // set the version set id
            if (creatorConfig.apiVersionSet != null)
            {
                apiTemplateResource.properties.apiVersionSetId = $"[resourceId('Microsoft.ApiManagement/service/api-version-sets', parameters('ApimServiceName'), '{versionSetId}')]";
            }
            else if (creatorConfig.api.apiVersionSetId != null)
            {
                apiTemplateResource.properties.apiVersionSetId = $"{creatorConfig.api.apiVersionSetId}";

            }
            return apiTemplateResource;
        }

        public APITemplateResource CreateSubsequentAPITemplateResource(CreatorConfig creatorConfig)
        {
            // create api resource with properties
            string subsequentAPIName = $"[concat(parameters('ApimServiceName'), '/{creatorConfig.api.name}')]";
            string subsequentAPIType = "Microsoft.ApiManagement/service/apis";
            Uri uriResult;
            bool isUrl = Uri.TryCreate(creatorConfig.api.openApiSpec, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            // used to escape sequences in local swagger json
            object deserializedFileContents = isUrl ? null : JsonConvert.DeserializeObject<object>(this.fileReader.RetrieveLocalFileContents(creatorConfig.api.openApiSpec));
            APITemplateResource apiTemplateResource = new APITemplateResource()
            {
                name = subsequentAPIName,
                type = subsequentAPIType,
                apiVersion = "2018-01-01",
                properties = new APITemplateProperties()
                {
                    contentFormat = isUrl ? "swagger-link-json" : "swagger-json",
                    contentValue = isUrl ? creatorConfig.api.openApiSpec : JsonConvert.SerializeObject(deserializedFileContents),
                    // supplied via optional arguments
                    path = creatorConfig.api.suffix
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
