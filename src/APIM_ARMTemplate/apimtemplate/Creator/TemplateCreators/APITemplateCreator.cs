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
        public async Task<APITemplate> CreateInitialAPITemplateAsync(CreatorConfig creatorConfig)
        {
            FileReader fileReader = new FileReader();
            // create api schema with properties
            APITemplate apiSchema = new APITemplate()
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
            return apiSchema;
        }

        public APITemplate CreateSubsequentAPITemplate(CreatorConfig creatorConfig)
        {
            // create api schema with properties
            APITemplate apiSchema = new APITemplate()
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
            return apiSchema;
        }
    }
}
