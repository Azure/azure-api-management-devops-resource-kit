// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Schemas;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class SchemaExtractor : ISchemaExtractor
    {
        readonly ILogger<SchemaExtractor> logger;
        readonly ISchemasClient schemaClient;
        readonly ITemplateBuilder templateBuilder;

        public SchemaExtractor(
            ILogger<SchemaExtractor> logger,
            ITemplateBuilder templateBuilder,
            ISchemasClient schemaClient)
        {
            this.logger = logger;
            this.templateBuilder = templateBuilder;
            this.schemaClient = schemaClient;
        }

        public async Task<Template<SchemaTemplateResources>> GenerateSchemasTemplateAsync(ExtractorParameters extractorParameters)
        {
            var schemasTemplate = this.templateBuilder
                                        .GenerateTemplateWithApimServiceNameProperty()
                                        .Build<SchemaTemplateResources>();

            var schemas = await this.schemaClient.GetAllAsync(extractorParameters);
            if (schemas.IsNullOrEmpty())
            {
                this.logger.LogWarning($"No schemas were found for '{extractorParameters.SourceApimName}' at '{extractorParameters.ResourceGroup}'");
                return schemasTemplate;
            }

            foreach (var schema in schemas)
            {
                schema.Type = ResourceTypeConstants.Schema;
                schema.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{schema.Name}')]";
                schema.ApiVersion = GlobalConstants.ApiVersion;
                schemasTemplate.TypedResources.Schemas.Add(schema);
            }

            return schemasTemplate;
        }
    }
}
