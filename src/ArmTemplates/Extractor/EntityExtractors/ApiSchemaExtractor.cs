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
                var apiSchemaOriginalName = apiSchema.Name;

                apiSchema.Properties.Document.Value = this.GetSchemaValueBasedOnContentType(apiSchema.Properties);
                apiSchema.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{apiSchemaOriginalName}')]";
                apiSchema.Type = ResourceTypeConstants.APISchema;
                apiSchema.ApiVersion = GlobalConstants.ApiVersion;
                apiSchema.DependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiName}')]" };

                apiSchemaResources.ApiSchemas.Add(apiSchema);
            }

            return apiSchemaResources;
        }

        string GetSchemaValueBasedOnContentType(ApiSchemaProperties schemaTemplateProperties)
        {
            if (schemaTemplateProperties is null)
            {
                return string.Empty;
            }

            var contentType = schemaTemplateProperties.ContentType.ToLowerInvariant();
            if (contentType.Equals("application/vnd.oai.openapi.components+json"))
            {
                // for OpenAPI "value" is not used, but "components" which is resolved during json deserialization
                return null;
            }

            var schemaValue = contentType switch
            {
                "application/vnd.ms-azure-apim.swagger.definitions+json" => schemaTemplateProperties?.Document?.Definitions?.Serialize(),
                "application/vnd.ms-azure-apim.xsd+xml" => schemaTemplateProperties?.Document?.Definitions?.Serialize(),
                _ => string.Empty
            };

            if (string.IsNullOrEmpty(schemaValue) && schemaTemplateProperties.Document is not null)
            {
                return schemaTemplateProperties.Document.Serialize();
            }

            return schemaValue;
        }
    }
}
