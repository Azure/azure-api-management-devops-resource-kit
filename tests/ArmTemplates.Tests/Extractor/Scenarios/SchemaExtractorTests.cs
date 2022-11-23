// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Schemas Extraction")]
    public class SchemaExtractorTests: ExtractorMockerWithOutputTestsBase
    {        
        public SchemaExtractorTests() : base("schemas-tests")
        {
        }

        [Fact]
        public async Task GenerateSchemasTemplates_ProperlyParsesResponse()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateSchemasTemplates_ProperlyParsesResponse));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration();
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var fileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListSchemas_success_response.json");
            var mockedSchemaClient = await MockSchemaClient.GetMockedHttpSchemaClient(new MockClientConfiguration(responseFileLocation: fileLocation));
            var schemaExtractor = new SchemaExtractor(this.GetTestLogger<SchemaExtractor>(), new TemplateBuilder(), mockedSchemaClient);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                schemaExtractor: schemaExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var schemaTemplate = await extractorExecutor.GenerateSchemasTemplateAsync(currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Schema)).Should().BeTrue();

            schemaTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            schemaTemplate.TypedResources.Schemas.Count().Should().Be(2);
            
            var xmlSchema = schemaTemplate.TypedResources.Schemas.First(x => x.OriginalName.Equals("schema1"));
            xmlSchema.Should().NotBeNull();
            xmlSchema.Properties.SchemaType.Should().Be("xml");
            xmlSchema.Properties.Description.Should().Be("sample schema description");
            xmlSchema.Properties.Document.Should().BeNull();
            xmlSchema.Properties.Value.Should().NotBeNull();

            var jsonSchema = schemaTemplate.TypedResources.Schemas.First(x => x.OriginalName.Equals("schema2"));
            jsonSchema.Should().NotBeNull();
            jsonSchema.Properties.SchemaType.Should().Be("json");
            jsonSchema.Properties.Description.Should().Be("sample schema description");
            jsonSchema.Properties.Document.Should().NotBeNull();
            jsonSchema.Properties.Value.Should().BeNull();
        }
    }
}
