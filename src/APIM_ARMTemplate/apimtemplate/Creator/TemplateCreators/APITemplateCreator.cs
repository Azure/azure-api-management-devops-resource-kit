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
        private TemplateCreator templateCreator;
        private FileReader fileReader;

        public APITemplateCreator(TemplateCreator templateCreator, FileReader fileReader)
        {
            this.templateCreator = templateCreator;
            this.fileReader = fileReader;
        }

        public Template CreateInitialAPITemplateAsync(CreatorConfig creatorConfig)
        {
            Template apiTemplate = this.templateCreator.CreateEmptyTemplate();

            List<TemplateResource> resources = new List<TemplateResource>();
            // create api resource with properties
            APITemplateResource apiTemplateResource = new APITemplateResource()
            {
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
                    apiVersionSet = creatorConfig.apiVersionSet != null ? new APITemplateVersionSet()
                    {
                        id = creatorConfig.apiVersionSet.id,
                        description = creatorConfig.apiVersionSet.description,
                        versionHeaderName = creatorConfig.apiVersionSet.versionHeaderName,
                        versionQueryName = creatorConfig.apiVersionSet.versionQueryName,
                        versioningScheme = creatorConfig.apiVersionSet.versioningScheme
                    } : null,
                    authenticationSettings = creatorConfig.api.authenticationSettings ?? null
                }
            };
            resources.Add(apiTemplateResource);

            apiTemplate.resources = resources.ToArray();
            return apiTemplate;
        }

        public async Task<Template> CreateSubsequentAPITemplate(CreatorConfig creatorConfig)
        {
            Template apiTemplate = this.templateCreator.CreateEmptyTemplate();

            List<TemplateResource> resources = new List<TemplateResource>();
            // create api resource with properties
            APITemplateResource apiTemplateResource = new APITemplateResource()
            {
                type = "Microsoft.ApiManagement/service/apis",
                apiVersion = "2018-06-01-preview",
                properties = new APITemplateProperties()
                {
                    contentFormat = "swagger-json",
                    contentValue = await this.fileReader.RetrieveLocationContentsAsync(creatorConfig.api.openApiSpec),
                    // supplied via optional arguments
                    path = creatorConfig.api.suffix ?? ""
                }
            };
            resources.Add(apiTemplateResource);

            apiTemplate.resources = resources.ToArray();
            return apiTemplate;
        }
    }
}
