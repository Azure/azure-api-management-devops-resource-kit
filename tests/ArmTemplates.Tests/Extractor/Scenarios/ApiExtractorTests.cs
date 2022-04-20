// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
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
using Moq;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Api Extraction")]
    public class ApiExtractorTests : ExtractorMockerWithOutputTestsBase
    {
        public ApiExtractorTests() : base("api-tests")
        {
        }

        [Fact]
        public async Task GenerateApiTemplates_ProperlyLaysTheInformation()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateApiTemplates_ProperlyLaysTheInformation));

            var extractorConfig = this.GetMockedExtractorConsoleAppConfiguration(
                splitApis: false,
                apiVersionSetName: string.Empty,
                multipleApiNames: string.Empty,
                includeAllRevisions: false);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            // mocked clients
            var mockedApiClient = MockApisClient.GetMockedApiClientWithDefaultValues();
            var mockedProductClient = MockProductsClient.GetMockedApiClientWithDefaultValues();
            var mockedApiSchemaClient = MockApiSchemaClient.GetMockedApiClientWithDefaultValues();
            var mockedPolicyClient = MockPolicyClient.GetMockedApiClientWithDefaultValues();
            var mockedTagClient = MockTagClient.GetMockedApiClientWithDefaultValues();
            var mockedApiOperationClient = MockApiOperationClient.GetMockedApiClientWithDefaultValues();
            var mockedDiagnosticClient = MockDiagnosticClient.GetMockedClientWithApiDependentValues();

            // mocked extractors
            var mockedDiagnosticExtractor = new DiagnosticExtractor(this.GetTestLogger<DiagnosticExtractor>(), mockedDiagnosticClient);
            var mockedApiSchemaExtractor = new ApiSchemaExtractor(this.GetTestLogger<ApiSchemaExtractor>(), mockedApiSchemaClient);
            var mockedPolicyExtractor = new PolicyExtractor(this.GetTestLogger<PolicyExtractor>(), mockedPolicyClient, new TemplateBuilder());
            var mockedProductApisExtractor = new ProductApisExtractor(this.GetTestLogger<ProductApisExtractor>(), mockedProductClient, mockedApiClient, new TemplateBuilder());
            var mockedTagExtractor = new TagExtractor(this.GetTestLogger<TagExtractor>(), mockedTagClient, new TemplateBuilder());
            var mockedApiOperationExtractor = new ApiOperationExtractor(this.GetTestLogger<ApiOperationExtractor>(), mockedApiOperationClient);

            var apiExtractor = new ApiExtractor(
                this.GetTestLogger<ApiExtractor>(),
                new TemplateBuilder(),
                mockedApiClient,
                mockedDiagnosticExtractor,
                mockedApiSchemaExtractor,
                mockedPolicyExtractor,
                mockedProductApisExtractor,
                mockedTagExtractor,
                mockedApiOperationExtractor);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                apiExtractor: apiExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var apiTemplate = await extractorExecutor.GenerateApiTemplateAsync(
                singleApiName: It.IsAny<string>(),
                multipleApiNames: It.IsAny<List<string>>(),
                currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, apiTemplate.TypedResources.FileName)).Should().BeTrue();
            Directory.GetFiles(Path.Combine(currentTestDirectory, PolicyExtractor.PoliciesDirectoryName)).Count().Should().Be(4);

            apiTemplate.Parameters.Should().NotBeNull();
            apiTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            apiTemplate.Parameters.Should().ContainKey(ParameterNames.ServiceUrl);
            apiTemplate.Parameters.Should().ContainKey(ParameterNames.ApiLoggerId);
            apiTemplate.Parameters.Should().ContainKey(ParameterNames.PolicyXMLBaseUrl);
            apiTemplate.Parameters.Should().ContainKey(ParameterNames.PolicyXMLSasToken);
            apiTemplate.Resources.Count().Should().Be(23);

            // apis
            apiTemplate.TypedResources.Apis.Count().Should().Be(2);
            apiTemplate.TypedResources.Apis.All(x => x.Type == ResourceTypeConstants.API).Should().BeTrue();
            apiTemplate.TypedResources.Apis.All(x => x.Properties is not null).Should().BeTrue();

            // api schemas
            apiTemplate.TypedResources.ApiSchemas.Count().Should().Be(2);
            apiTemplate.TypedResources.ApiSchemas.All(x => x.Type == ResourceTypeConstants.APISchema).Should().BeTrue();
            apiTemplate.TypedResources.ApiSchemas.All(x => x.Properties is not null).Should().BeTrue();

            // diagnostics
            apiTemplate.TypedResources.Diagnostics.Count().Should().Be(3);
            apiTemplate.TypedResources.Diagnostics.All(x => x.Type == ResourceTypeConstants.APIServiceDiagnostic || x.Type == ResourceTypeConstants.APIDiagnostic).Should().BeTrue();
            apiTemplate.TypedResources.Diagnostics.All(x => x.Properties is not null).Should().BeTrue();

            // tags
            apiTemplate.TypedResources.Tags.Count().Should().Be(4);
            apiTemplate.TypedResources.Tags.All(x => x.Type == ResourceTypeConstants.ProductTag).Should().BeTrue();

            // api products
            apiTemplate.TypedResources.ApiProducts.Count().Should().Be(2);
            apiTemplate.TypedResources.ApiProducts.All(x => x.Type == ResourceTypeConstants.ProductApi).Should().BeTrue();
            apiTemplate.TypedResources.ApiProducts.All(x => x.Properties is not null).Should().BeTrue();

            // api policies
            apiTemplate.TypedResources.ApiPolicies.Count().Should().Be(2);
            apiTemplate.TypedResources.ApiPolicies.All(x => x.Properties is not null).Should().BeTrue();

            // api operations
            apiTemplate.TypedResources.ApiOperations.Count().Should().Be(2);
            apiTemplate.TypedResources.ApiOperations.All(x => x.Type == ResourceTypeConstants.APIOperation).Should().BeTrue();
            apiTemplate.TypedResources.ApiOperations.All(x => x.Properties is not null).Should().BeTrue();
            apiTemplate.TypedResources.ApiOperations.SelectMany(x => x.DependsOn).Any(x => x.Contains($"'{ResourceTypeConstants.API}'")).Should().BeTrue();
            apiTemplate.TypedResources.ApiOperations.SelectMany(x => x.DependsOn).Any(x => x.Contains($"'{ResourceTypeConstants.APIOperation}'")).Should().BeFalse();

            // api operations policies
            apiTemplate.TypedResources.ApiOperationsPolicies.Count().Should().Be(2);
            apiTemplate.TypedResources.ApiOperations.All(x => x.Properties is not null).Should().BeTrue();

            // api operations tags
            apiTemplate.TypedResources.ApiOperationsPolicies.Count().Should().Be(2);
            apiTemplate.TypedResources.ApiOperations.All(x => x.Properties is not null).Should().BeTrue();
        }
    }
}
