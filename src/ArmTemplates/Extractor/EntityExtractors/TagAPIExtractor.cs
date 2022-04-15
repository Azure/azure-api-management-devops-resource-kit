// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.TagApi;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class TagApiExtractor : ITagApiExtractor
    {
        readonly ILogger<TagApiExtractor> logger;
        readonly ITemplateBuilder templateBuilder;
        readonly IApisClient apisClient;
        readonly ITagClient tagClient;

        public TagApiExtractor(
            ILogger<TagApiExtractor> logger,
            ITemplateBuilder templateBuilder,
            IApisClient apisClient,
            ITagClient tagClient)
        {
            this.logger = logger;
            this.templateBuilder = templateBuilder;

            this.tagClient = tagClient;
            this.apisClient = apisClient;
        }

        public async Task<Template<TagApiTemplateResources>> GenerateApiTagsTemplateAsync(
            string singleApiName,
            List<string> multipleApiNames,
            ExtractorParameters extractorParameters)
        {
            var tagApiTemplate = this.templateBuilder
                                        .GenerateTemplateWithApimServiceNameProperty()
                                        .Build<TagApiTemplateResources>();

            if (!string.IsNullOrEmpty(singleApiName))
            {
                tagApiTemplate.TypedResources = await this.GenerateSingleApiTagResourceAsync(singleApiName, extractorParameters, Array.Empty<string>());
            }
            else if (!multipleApiNames.IsNullOrEmpty())
            {
                tagApiTemplate.TypedResources = await this.GenerateMultipleApisTemplateAsync(multipleApiNames, extractorParameters);
            }
            else
            {
                var serviceApis = await this.apisClient.GetAllAsync(extractorParameters);
                this.logger.LogDebug("{0} APIs found ...", serviceApis.Count);

                var serviceApiNames = serviceApis.Select(api => api.Name).ToList();
                tagApiTemplate.TypedResources = await this.GenerateMultipleApisTemplateAsync(serviceApiNames, extractorParameters);
            }

            return tagApiTemplate;
        }

        async Task<TagApiTemplateResources> GenerateMultipleApisTemplateAsync(List<string> multipleApiNames, ExtractorParameters extractorParameters)
        {
            this.logger.LogDebug("Processing {0} api-names...", multipleApiNames.Count);

            var overallResources = new TagApiTemplateResources();
            string[] dependsOn = Array.Empty<string>();
            foreach (string apiName in multipleApiNames)
            {
                var tagApiResources = await this.GenerateSingleApiTagResourceAsync(apiName, extractorParameters, dependsOn);

                if (tagApiResources.HasContent())
                {
                    overallResources.AddDataResources(tagApiResources);

                    // Extract the tag name from the last resource
                    string[] lastTagName = tagApiResources.Tags.Last().Name.Replace("')]", "").Split('/');
                    dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis/tags', parameters('{ParameterNames.ApimServiceName}'), '{apiName}', '{lastTagName[2]}')]" };
                }
            }

            return overallResources;
        }

        async Task<TagApiTemplateResources> GenerateSingleApiTagResourceAsync(string apiName, ExtractorParameters extractorParameters, string[] dependsOn)
        {
            var templateResources = new TagApiTemplateResources();

            var apiTags = await this.tagClient.GetAllTagsLinkedToApiAsync(apiName, extractorParameters);
            var dependencyChain = dependsOn;
            foreach (var apiTag in apiTags)
            {
                var apiTagOriginalName = apiTag.Name;

                apiTag.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{apiTagOriginalName}')]";
                apiTag.ApiVersion = GlobalConstants.ApiVersion;
                apiTag.Scale = null;
                apiTag.DependsOn = dependencyChain;
                dependencyChain = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis/tags', parameters('{ParameterNames.ApimServiceName}'), '{apiName}', '{apiTagOriginalName}')]" };

                templateResources.Tags.Add(apiTag);
            }

            return templateResources;
        }
    }
}