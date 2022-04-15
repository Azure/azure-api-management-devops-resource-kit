// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class TagExtractor : ITagExtractor
    {
        readonly ILogger<TagExtractor> logger;
        readonly ITagClient tagClient;
        readonly ITemplateBuilder templateBuilder;

        public TagExtractor(
            ILogger<TagExtractor> logger,
            ITagClient tagClient,
            ITemplateBuilder templateBuilder)
        {
            this.logger = logger;
            this.tagClient = tagClient;
            this.templateBuilder = templateBuilder;
        }

        public async Task<List<TagTemplateResource>> GenerateTagResourcesLinkedToApiOperationAsync(string apiName, string operationName, ExtractorParameters extractorParameters)
        {
            var apiOperationTags = await this.tagClient.GetTagsLinkedToApiOperationAsync(apiName, operationName, extractorParameters);

            if (apiOperationTags.IsNullOrEmpty())
            {
                this.logger.LogWarning("No tags found for api '{0}' and operation {1}", apiName, operationName);
                return apiOperationTags;
            }

            var templateResources = new List<TagTemplateResource>();
            foreach (var apiOperationTag in apiOperationTags)
            {
                var apiOperationTagOriginalName = apiOperationTag.Name;

                apiOperationTag.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{operationName}/{apiOperationTagOriginalName}')]";
                apiOperationTag.ApiVersion = GlobalConstants.ApiVersion;
                apiOperationTag.Scale = null;
                apiOperationTag.DependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis/operations', parameters('{ParameterNames.ApimServiceName}'), '{apiName}', '{operationName}')]" };
                
                templateResources.Add(apiOperationTag);
            }

            return templateResources;
        }

        public async Task<List<TagTemplateResource>> GenerateTagResourcesLinkedToApiAsync(string apiName, ExtractorParameters extractorParameters)
        {
            var apiTags = await this.tagClient.GetAllTagsLinkedToApiAsync(apiName, extractorParameters);

            if (apiTags.IsNullOrEmpty())
            {
                this.logger.LogWarning("No tags found for api {0}", apiName);
                return apiTags;
            }

            var templateResources = new List<TagTemplateResource>();
            foreach (var apiTag in apiTags)
            {
                var apiTagOriginalName = apiTag.Name;

                apiTag.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{apiTagOriginalName}')]";
                apiTag.ApiVersion = GlobalConstants.ApiVersion;
                apiTag.Scale = null;
                apiTag.DependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiName}')]" };

                templateResources.Add(apiTag);
            }

            return templateResources;
        }

        public async Task<Template<TagTemplateResources>> GenerateTagsTemplateAsync(
            string singleApiName,
            ApiTemplateResources apiTemplateResources,
            ProductTemplateResources productTemplateResources,
            ExtractorParameters extractorParameters)
        {
            var tagTemplate = this.templateBuilder
                                    .GenerateTemplateWithApimServiceNameProperty()
                                    .Build<TagTemplateResources>();

            // isolate tag and api operation associations in the case of a single api extraction
            var apiOperationTagResources = apiTemplateResources.ApiOperationsTags;

            // isolate product api associations in the case of a single api extraction
            var productAPIResources = apiTemplateResources.ApiProducts;

            // isolate tag and product associations in the case of a single api extraction
            var productTagResources = productTemplateResources.Tags;

            var tags = await this.tagClient.GetAllAsync(extractorParameters);
            foreach (var tag in tags)
            {
                var tagOriginalName = tag.Name;

                tag.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{tagOriginalName}')]";
                tag.Type = ResourceTypeConstants.Tag;
                tag.ApiVersion = GlobalConstants.ApiVersion;
                tag.Scale = null;

                // only extract the tag if this is a full extraction, 
                // or in the case of a single api, if it is found in tags associated with the api operations
                // or if it is found in tags associated with the api
                // or if it is found in tags associated with the products associated with the api
                if (string.IsNullOrEmpty(singleApiName)
                        || apiOperationTagResources.Any(t => t.Name.Contains($"/{tag}'"))
                        || productAPIResources.Any(t => t.Name.Contains($"/{singleApiName}"))
                            && productTagResources.Any(t => t.Name.Contains($"/{tagOriginalName}'")))
                {
                    tagTemplate.TypedResources.Tags.Add(tag);
                }
            }

            return tagTemplate;
        }
    }
}