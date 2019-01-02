﻿using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class APITemplateCreator
    {
        public async Task<APITemplate> CreateAPITemplateAsync(CreatorConfig creatorConfig)
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
                    contentValue = await yamlReader.RetrieveLocationContents(creatorConfig.api.openApiSpec),
                    // supplied via optional arguments
                    apiVersion = creatorConfig.api.apiVersion ?? "",
                    apiRevision = creatorConfig.api.revision ?? "",
                    apiVersionSetId = creatorConfig.api.versionSetId ?? "",
                    path = creatorConfig.api.suffix ?? "",
                    apiRevisionDescription = creatorConfig.api.revisionDescription ?? "",
                    apiVersionDescription = creatorConfig.api.apiVersionDescription ?? "",
                    apiVersionSet = creatorConfig.apiVersionSet ?? null,
                    authenticationSettings = creatorConfig.api.authenticationSettings ?? null
                }
            };
            return apiSchema;
        }
    }
}
