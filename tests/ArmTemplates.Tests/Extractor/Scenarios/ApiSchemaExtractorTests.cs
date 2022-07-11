// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Api Schema Extraction")]
    public class ApiSchemaExtractorTests : ExtractorMockerWithOutputTestsBase
    {
        public ApiSchemaExtractorTests() : base("api-schema-tests")
        {
        }

        [Fact]
        public async Task GenerateApiTemplates_ProperlyParsesTheInformation()
        {
            // arrange
            var responseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListApiSchemas_success_response.json");

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration();
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var mockedApiSchemaClient = await MockApiSchemaClient.GetMockedHttpApiSchemaClient(responseFileLocation);
            var mockedApiSchemaExtractor = new ApiSchemaExtractor(this.GetTestLogger<ApiSchemaExtractor>(), mockedApiSchemaClient);

            // act
            var apiSchemas = await mockedApiSchemaExtractor.GenerateApiSchemaResourcesAsync("apiName", extractorParameters);

            // assert
            apiSchemas.ApiSchemas.Count().Should().Be(2);
            apiSchemas.ApiSchemas.All(x => x.Type == ResourceTypeConstants.APISchema).Should().BeTrue();
            apiSchemas.ApiSchemas.All(x => x.Properties is not null).Should().BeTrue();

            var jsonSchema = apiSchemas.ApiSchemas.First(x=>x.Name.Contains("schemaName1"));
            jsonSchema.Should().NotBeNull();
            jsonSchema.Properties.Document.Value.Should().BeNull();

            var xmlSchema = apiSchemas.ApiSchemas.First(x => x.Name.Contains("schemaName2"));
            xmlSchema.Should().NotBeNull();
            xmlSchema.Properties.Document.Value.Should().NotBeNull();
            xmlSchema.Properties.Document.Definitions.Should().BeNull();
            xmlSchema.Properties.Document.Components.Should().BeNull();
        }
    }
}
