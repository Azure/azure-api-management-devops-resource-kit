// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiReleases;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class ApiReleaseExtractor : IApiReleaseExtractor
    {
        readonly ILogger<ApiReleaseExtractor> logger;
        readonly ITemplateBuilder templateBuilder;
        readonly IApisClient apisClient;

        public ApiReleaseExtractor(
            ILogger<ApiReleaseExtractor> logger,
            ITemplateBuilder templateBuilder,
            IApisClient apisClient
            )
        {
            this.logger = logger;
            this.templateBuilder = templateBuilder;
            this.apisClient = apisClient;
        }

        public Template<ApiReleaseTemplateResources> GenerateSingleApiReleaseTemplate(string apiName, ExtractorParameters extractorParameters)
        {
            var apiReleaseTemplate = this.templateBuilder
                                        .GenerateTemplateWithApimServiceNameProperty()
                                        .Build<ApiReleaseTemplateResources>();

            var apiReleaseResource = this.GenerateReleaseResource(apiName);
            apiReleaseTemplate.TypedResources.ApiReleases.Add(apiReleaseResource);
            
            return apiReleaseTemplate;
        }


        public async Task<Template<ApiReleaseTemplateResources>> GenerateCurrentApiReleaseTemplate(ExtractorParameters extractorParameters)
        {
            var apiReleasesTemplate = this.templateBuilder
                                        .GenerateTemplateWithApimServiceNameProperty()
                                        .Build<ApiReleaseTemplateResources>();

            var apis = await this.apisClient.GetAllCurrentAsync(extractorParameters);

            if (apis.IsNullOrEmpty())
            {
                this.logger.LogWarning($"No current apis were found for '{extractorParameters.SourceApimName}' at '{extractorParameters.ResourceGroup}'");
                return apiReleasesTemplate;
            }

            foreach (var api in apis)
            {
                var apiReleaseResource = this.GenerateReleaseResource(api.ApiNameWithRevision);
                apiReleasesTemplate.TypedResources.ApiReleases.Add(apiReleaseResource);
            }
            
            return apiReleasesTemplate;
        }

        ApiReleaseTemplateResource GenerateReleaseResource(string apiName)
        {
            var apiReleaseName = $"{Guid.NewGuid()}";
            var apiReleaseResource = new ApiReleaseTemplateResource
            {
                Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{apiReleaseName}')]",
                Type = ResourceTypeConstants.APIRelease,
                ApiVersion = GlobalConstants.ApiVersion,
                Properties = new ApiReleaseProperties
                {
                    ApiId = $"/apis/{apiName}"
                }
            };

            return apiReleaseResource;
        }
    }
}
