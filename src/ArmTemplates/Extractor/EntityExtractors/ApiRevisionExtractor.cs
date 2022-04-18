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

        async Task<ApiTemplateResources> GenerateCurrentRevisionApiTemplateResourcesAsync(string apiName, string baseFilesGenerationDirectory, ExtractorParameters extractorParameters)
        {
            var apiTemplateResources = new ApiTemplateResources();

            // gets the current revision of this api and will remove "isCurrent" paramter
            var versionedApiResource = await this.GenerateApiTemplateResourceAsVersioned(apiName, extractorParameters);

            apiTemplateResources.Apis.Add(versionedApiResource);
            var relatedTemplateResources = await this.apiExtractor.GetApiRelatedTemplateResourcesAsync(apiName, baseFilesGenerationDirectory, extractorParameters);
            apiTemplateResources.AddResourcesData(relatedTemplateResources);

            return apiTemplateResources;
        }

        async Task<ApiTemplateResource> GenerateApiTemplateResourceAsVersioned(string apiName, ExtractorParameters extractorParameters)
        {
            var apiDetails = await this.apisClient.GetSingleAsync(apiName, extractorParameters);

            apiDetails.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}')]";
            apiDetails.ApiVersion = GlobalConstants.ApiVersion;
            apiDetails.Scale = null;
            apiDetails.Properties.IsCurrent = null;

            if (extractorParameters.ParameterizeServiceUrl)
            {
                apiDetails.Properties.ServiceUrl = $"[parameters('{ParameterNames.ServiceUrl}').{ParameterNamingHelper.GenerateValidParameterName(apiName, ParameterPrefix.Api)}]";
            }

            if (apiDetails.Properties.ApiVersionSetId != null)
            {
                apiDetails.DependsOn = Array.Empty<string>();

                string versionSetName = apiDetails.Properties.ApiVersionSetId;
                int versionSetPosition = versionSetName.IndexOf("apiVersionSets/");

                versionSetName = versionSetName.Substring(versionSetPosition, versionSetName.Length - versionSetPosition);
                apiDetails.Properties.ApiVersionSetId = $"[concat(resourceId('Microsoft.ApiManagement/service', parameters('{ParameterNames.ApimServiceName}')), '/{versionSetName}')]";
            }
            else
            {
                apiDetails.DependsOn = Array.Empty<string>();
            }

            return apiDetails;
        }
    }
}
