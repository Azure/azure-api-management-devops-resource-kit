using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
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

        public async Task<Template> CreateAPITemplateAsync(CreatorConfig creatorConfig)
        {
            Template apiTemplate = this.templateCreator.CreateEmptyTemplate();

            // add parameters
            apiTemplate.parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { "ApimServiceName", new TemplateParameterProperties(){ type = "string" } }
            };

            string subsequentAPIName = "[concat(parameters('ApimServiceName'), '/api')]";
            string subsequentAPIType = "Microsoft.ApiManagement/service/apis";
            string[] dependsOnSubsequentAPI = new string[] { $"[resourceId('{subsequentAPIType}', '{subsequentAPIName}')]" };

            List<TemplateResource> resources = new List<TemplateResource>();
            // create api resource with properties
            APITemplateResource initialAPITemplateResource = this.CreateInitialAPITemplateResource(creatorConfig);
            APITemplateResource subsequentAPITemplateResource = await this.CreateSubsequentAPITemplateResourceAsync(creatorConfig);
            PolicyTemplateResource apiPolicyResource = await this.policyTemplateCreator.CreateAPIPolicyTemplateResourceAsync(creatorConfig, dependsOnSubsequentAPI);
            List<PolicyTemplateResource> operationPolicyResources = await this.policyTemplateCreator.CreateOperationPolicyTemplateResourcesAsync(creatorConfig, dependsOnSubsequentAPI);
            List<ProductAPITemplateResource> productAPIResources = this.productAPITemplateCreator.CreateProductAPITemplateResources(creatorConfig, dependsOnSubsequentAPI);
            resources.Add(initialAPITemplateResource);
            resources.Add(subsequentAPITemplateResource);
            resources.Add(apiPolicyResource);
            resources.AddRange(operationPolicyResources);
            resources.AddRange(productAPIResources);

            apiTemplate.resources = resources.ToArray();
            return apiTemplate;
        }

        public APITemplateResource CreateInitialAPITemplateResource(CreatorConfig creatorConfig)
        {
            // create api resource with properties
            APITemplateResource apiTemplateResource = new APITemplateResource()
            {
                name = "[concat(parameters('ApimServiceName'), '/api')]",
                type = "Microsoft.ApiManagement/service/apis",
                apiVersion = "2018-06-01-preview",
                properties = new APITemplateProperties()
                {
                    // supplied via optional arguments
                    apiVersion = creatorConfig.api.apiVersion ?? "",
                    apiRevision = creatorConfig.api.revision ?? "",
                    apiVersionSetId = creatorConfig.api.versionSetId ?? "",
                    apiRevisionDescription = creatorConfig.api.revisionDescription ?? "",
                    apiVersionDescription = creatorConfig.api.apiVersionDescription ?? "",
                    authenticationSettings = creatorConfig.api.authenticationSettings ?? null
                },
                dependsOn = new string[] { }
            };
            return apiTemplateResource;
        }

        public async Task<APITemplateResource> CreateSubsequentAPITemplateResourceAsync(CreatorConfig creatorConfig)
        {
            // create api resource with properties
            // used to escape characters in json file
            object deserializedFileContents = JsonConvert.DeserializeObject<object>(await this.fileReader.RetrieveLocationContentsAsync(creatorConfig.api.openApiSpec));
            string subsequentAPIName = "[concat(parameters('ApimServiceName'), '/api')]";
            string subsequentAPIType = "Microsoft.ApiManagement/service/apis";
            APITemplateResource apiTemplateResource = new APITemplateResource()
            {
                name = subsequentAPIName,
                type = subsequentAPIType,
                apiVersion = "2018-06-01-preview",
                properties = new APITemplateProperties()
                {
                    contentFormat = "swagger-json",
                    contentValue = JsonConvert.SerializeObject(deserializedFileContents),
                    // supplied via optional arguments
                    path = creatorConfig.api.suffix ?? ""
                },
                dependsOn = new string[] { $"[resourceId('{subsequentAPIType}', '{subsequentAPIName}')]" }
        };
            return apiTemplateResource;
        }
    }
}
