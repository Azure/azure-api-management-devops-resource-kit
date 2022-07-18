// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
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

        public ApiReleaseExtractor(
            ILogger<ApiReleaseExtractor> logger,
            ITemplateBuilder templateBuilder)
        {
            this.logger = logger;
            this.templateBuilder = templateBuilder;
        }

        public Template<ApiReleaseTemplateResources> GenerateSingleApiReleaseTemplate(string apiName, ExtractorParameters extractorParameters)
        {
            var apiReleaseTemplate = this.templateBuilder
                                        .GenerateTemplateWithApimServiceNameProperty()
                                        .Build<ApiReleaseTemplateResources>();
            var currentDatetime = DateTime.Now.ToString("hhmmssddMMyy");
            var apiReleaseName = $"{currentDatetime}release";

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
            apiReleaseTemplate.TypedResources.ApiReleases.Add(apiReleaseResource);
            
            return apiReleaseTemplate;
        }
    }
}
