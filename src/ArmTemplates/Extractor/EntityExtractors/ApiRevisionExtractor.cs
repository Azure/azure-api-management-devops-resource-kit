﻿// --------------------------------------------------------------------------
//  <copyright file="ApiRevisionExtractor.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiRevisions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class ApiRevisionExtractor : IApiRevisionExtractor
    {
        readonly ILogger<ApiRevisionExtractor> logger;
        readonly ITemplateBuilder templateBuilder;

        readonly IApisClient apisClient;
        readonly IApiRevisionClient apiRevisionClient;

        readonly IApiExtractor apiExtractor;

        public ApiRevisionExtractor(
            ILogger<ApiRevisionExtractor> logger,
            ITemplateBuilder templateBuilder,
            IApiExtractor apiExtractor,
            IApisClient apisClient,
            IApiRevisionClient apiRevisionClient)
        {
            this.logger = logger;
            this.templateBuilder = templateBuilder;

            this.apisClient = apisClient;
            this.apiRevisionClient = apiRevisionClient;

            this.apiExtractor = apiExtractor;
        }

        public async IAsyncEnumerable<ApiRevisionTemplateResource> GetApiRevisionsAsync(string apiName, ExtractorParameters extractorParameters)
        {
            var apiRevisions = await this.apiRevisionClient.GetApiRevisionsAsync(apiName, extractorParameters);

            if (apiRevisions.IsNullOrEmpty())
            {
                this.logger.LogWarning("No api revisions found for api {0}", apiName);
                yield break;
            }

            foreach (var apiRevision in apiRevisions)
            {
                yield return apiRevision;
            }
        }

        public async Task<Template<ApiTemplateResources>> GenerateApiRevisionTemplateAsync(
            string currentRevision, 
            List<string> revList, 
            string baseFilesGenerationDirectory, 
            ExtractorParameters extractorParameters)
        {
            var apiRevisionTemplate = this.templateBuilder
                                            .GenerateTemplateWithPresetProperties(extractorParameters)
                                            .Build<ApiTemplateResources>();

            foreach (string curApi in revList)
            {
                // should add current api to dependsOn to those revisions that are not "current"
                if (curApi.Equals(currentRevision))
                {
                    var apiResources = await this.apiExtractor.GenerateSingleApiTemplateResourcesAsync(currentRevision, baseFilesGenerationDirectory, extractorParameters);
                    apiRevisionTemplate.TypedResources.AddResourcesData(apiResources);
                }
                else
                {
                    var apiResources = await this.apiExtractor.GenerateSingleApiTemplateResourcesAsync(curApi, baseFilesGenerationDirectory, extractorParameters);
                    var apiResource = apiResources.Apis.FirstOrDefault();
                    apiResource.DependsOn = new[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{extractorParameters.SingleApiName}')]" };

                    apiRevisionTemplate.TypedResources.AddResourcesData(apiResources);
                }
            }

            return apiRevisionTemplate;
        }
    }
}
