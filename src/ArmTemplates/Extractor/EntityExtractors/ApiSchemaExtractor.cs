// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiSchemas;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class ApiSchemaExtractor : IApiSchemaExtractor
    {
        readonly ILogger<ApiSchemaExtractor> logger;
        readonly IApiSchemaClient apiSchemaClient;

        public ApiSchemaExtractor(
            ILogger<ApiSchemaExtractor> logger,
            IApiSchemaClient schemaClient)
        {
            this.logger = logger;
            this.apiSchemaClient = schemaClient;
        }

        public async Task<ApiSchemaTemplateResources> GenerateApiSchemaResourcesAsync(string apiName, ExtractorParameters extractorParameters)
        {
            var apiSchemaResources = new ApiSchemaTemplateResources();
            var apiSchemas = await this.apiSchemaClient.GetApiSchemasAsync(apiName, extractorParameters);

            foreach (var apiSchema in apiSchemas)
            {
                apiSchema.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{apiSchema.OriginalName}')]";
                apiSchema.Type = ResourceTypeConstants.APISchema;
                apiSchema.ApiVersion = GlobalConstants.ApiVersion;
                apiSchema.DependsOn = new string[] { NamingHelper.GenerateApisResourceId(apiName) };

                apiSchemaResources.ApiSchemas.Add(apiSchema);
            }

            return apiSchemaResources;
        }
    }
}
