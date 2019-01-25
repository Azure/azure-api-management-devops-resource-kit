using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class APITemplateCreator
    {
        private FileReader fileReader;
        private TemplateCreator templateCreator;
        private PolicyTemplateCreator policyTemplateCreator;
        private ProductAPITemplateCreator productAPITemplateCreator;

        public APITemplateCreator(FileReader fileReader, TemplateCreator templateCreator, PolicyTemplateCreator policyTemplateCreator, ProductAPITemplateCreator productAPITemplateCreator)
        {
            this.fileReader = fileReader;
            this.templateCreator = templateCreator;
            this.policyTemplateCreator = policyTemplateCreator;
            this.productAPITemplateCreator = productAPITemplateCreator;
        }

        public APITemplateCreator(TemplateCreator templateCreator)
        {
            this.templateCreator = templateCreator;
        }

        public async Task<Template> CreateAPITemplateAsync(CreatorConfig creatorConfig)
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
            // create api resource with properties
            APITemplateResource initialAPITemplateResource = await this.CreateInitialAPITemplateResourceEx(creatorConfig);
            //APITemplateResource subsequentAPITemplateResource = await this.CreateSubsequentAPITemplateResourceAsync(creatorConfig);
            //PolicyTemplateResource apiPolicyResource = await this.policyTemplateCreator.CreateAPIPolicyTemplateResourceAsync(creatorConfig, dependsOnSubsequentAPI);
            //List<PolicyTemplateResource> operationPolicyResources = await this.policyTemplateCreator.CreateOperationPolicyTemplateResourcesAsync(creatorConfig, dependsOnSubsequentAPI);
            //List<ProductAPITemplateResource> productAPIResources = this.productAPITemplateCreator.CreateProductAPITemplateResources(creatorConfig, dependsOnSubsequentAPI);
            resources.Add(initialAPITemplateResource);
            //resources.Add(subsequentAPITemplateResource);
            //resources.Add(apiPolicyResource);
            //resources.AddRange(operationPolicyResources);
            //resources.AddRange(productAPIResources);
            
            apiTemplate.resources = resources.ToArray();
            return apiTemplate;
        }
        public async Task<Template> CreateAPITemplateAsync(JObject data)
        {
            // create empty template
            Template apiTemplate = this.templateCreator.CreateEmptyTemplate();

            // add parameters
            apiTemplate.parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { "ApimServiceName", new TemplateParameterProperties(){ type = "string" } }
            };

            string apiName = (string)data["value"][0]["name"];
            string[] dependsOnSubsequentAPI = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{apiName}')]" };

            List<TemplateResource> resources = new List<TemplateResource>();
            // create api resource with properties
            //APITemplateResource initialAPITemplateResource = await this.CreateInitialAPITemplateResource(creatorConfig);
            //APITemplateResource subsequentAPITemplateResource = await this.CreateSubsequentAPITemplateResourceAsync(creatorConfig);
            //PolicyTemplateResource apiPolicyResource = await this.policyTemplateCreator.CreateAPIPolicyTemplateResourceAsync(creatorConfig, dependsOnSubsequentAPI);
            //List<PolicyTemplateResource> operationPolicyResources = await this.policyTemplateCreator.CreateOperationPolicyTemplateResourcesAsync(creatorConfig, dependsOnSubsequentAPI);
            //List<ProductAPITemplateResource> productAPIResources = this.productAPITemplateCreator.CreateProductAPITemplateResources(creatorConfig, dependsOnSubsequentAPI);
            //resources.Add(initialAPITemplateResource);
            //resources.Add(subsequentAPITemplateResource);
            //resources.Add(apiPolicyResource);
            //resources.AddRange(operationPolicyResources);
            //resources.AddRange(productAPIResources);

            apiTemplate.resources = resources.ToArray();
            return apiTemplate;
        }

        public async Task<APITemplateResource> CreateInitialAPITemplateResource(CreatorConfig creatorConfig)
        {
            // protocols can be pulled by converting the OpenApiSpec into the OpenApiDocument class
            OpenAPISpecReader openAPISpecReader = new OpenAPISpecReader();
            OpenApiDocument doc = await openAPISpecReader.ConvertOpenAPISpecToDoc(creatorConfig.api.openApiSpec);

            // create api resource with properties
            APITemplateResource apiTemplateResource = new APITemplateResource()
            {
                name = $"[concat(parameters('ApimServiceName'), '/{creatorConfig.api.name}-initial')]",
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
                dependsOn = creatorConfig.linked == true ? new string[] { } : new string[] { $"[resourceId('Microsoft.ApiManagement/service/api-version-sets', parameters('ApimServiceName'), 'versionset')]" }
            };
            if (creatorConfig.apiVersionSet != null)
            {
                apiTemplateResource.properties.apiVersionSetId = "[resourceId('Microsoft.ApiManagement/service/api-version-sets', parameters('ApimServiceName'), 'versionset')]";
            }
            return apiTemplateResource;
        }

        public async Task<APITemplateResource> CreateInitialAPITemplateResourceEx(CreatorConfig creatorConfig)
        {
            // create api resource with properties
            List<string> protocols = new List<string>();
            protocols.Add("http");
            protocols.Add("https");
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
                    protocols = protocols.ToArray()
                },
                // if the template is not linked the depends on for the apiVersionSet needs to be inlined here
                //dependsOn = creatorConfig.linked == true ? new string[] { } : new string[] { $"[resourceId('Microsoft.ApiManagement/service/api-version-sets', parameters('ApimServiceName'), 'versionset')]" }
            };
            if (creatorConfig.apiVersionSet != null)
            {
                apiTemplateResource.properties.apiVersionSetId = "[resourceId('Microsoft.ApiManagement/service/api-version-sets', parameters('ApimServiceName'), 'versionset')]";
            }
            return apiTemplateResource;
        }

        public async Task<APITemplateResource> CreateSubsequentAPITemplateResourceAsync(CreatorConfig creatorConfig)
        {
            // create api resource with properties
            string subsequentAPIName = $"[concat(parameters('ApimServiceName'), '/{creatorConfig.api.name}')]";
            string subsequentAPIType = "Microsoft.ApiManagement/service/apis";
            object deserializedFileContents = null;
            if (creatorConfig.api.openApiSpec != null)
            {
                deserializedFileContents = JsonConvert.DeserializeObject<object>(await this.fileReader.RetrieveJSONContentsAsync(creatorConfig.api.openApiSpec));
            }
            APITemplateResource apiTemplateResource = new APITemplateResource()
            {
                name = subsequentAPIName,
                type = subsequentAPIType,
                apiVersion = "2018-01-01",
                properties = new APITemplateProperties()
                {
                    contentFormat = "swagger-json",
                    contentValue = deserializedFileContents != null ? JsonConvert.SerializeObject(deserializedFileContents) : null,
                    // supplied via optional arguments
                    path = creatorConfig.api.suffix
                },
                dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{creatorConfig.api.name}-initial')]" }
            };
            return apiTemplateResource;
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
