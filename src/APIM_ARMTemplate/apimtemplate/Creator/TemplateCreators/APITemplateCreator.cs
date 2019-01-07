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

        public Template CreateInitialAPITemplateAsync(CreatorConfig creatorConfig)
        {
            Template apiTemplate = new Template()
            {
                schema = "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
                contentVersion = "1.0.0.0",
                parameters = { },
                variables = { },
                resources = new TemplateResource[] { },
                outputs = { }
            };

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
            Template apiTemplate = new Template()
            {
                schema = "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
                contentVersion = "1.0.0.0",
                parameters = { },
                variables = { },
                resources = new TemplateResource[] { },
                outputs = { }
            };

            FileReader fileReader = new FileReader();
            List<TemplateResource> resources = new List<TemplateResource>();
            // create api resource with properties
            APITemplateResource apiTemplateResource = new APITemplateResource()
            {
                type = "Microsoft.ApiManagement/service/apis",
                apiVersion = "2018-06-01-preview",
                properties = new APITemplateProperties()
                {
                    contentFormat = "swagger-json",
                    contentValue = await fileReader.RetrieveLocationContentsAsync(creatorConfig.api.openApiSpec),
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
