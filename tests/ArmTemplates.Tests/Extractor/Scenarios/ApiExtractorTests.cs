// --------------------------------------------------------------------------
//  <copyright file="ApiExtractorTests.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
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
            var mockedDiagnosticClient = MockDiagnosticClient.GetMockedApiClientWithDefaultValues();

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
            File.Exists(Path.Combine(currentTestDirectory, apiTemplate.SpecificResources.FileName)).Should().BeTrue();
            Directory.GetFiles(Path.Combine(currentTestDirectory, PolicyExtractor.PoliciesDirectoryName)).Count().Should().Be(4);

            apiTemplate.Parameters.Should().NotBeNull();
            apiTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            apiTemplate.Parameters.Should().ContainKey(ParameterNames.ServiceUrl);
            apiTemplate.Parameters.Should().ContainKey(ParameterNames.ApiLoggerId);
            apiTemplate.Parameters.Should().ContainKey(ParameterNames.PolicyXMLBaseUrl);
            apiTemplate.Parameters.Should().ContainKey(ParameterNames.PolicyXMLSasToken);
            apiTemplate.Resources.Count().Should().Be(23);

            // apis
            apiTemplate.SpecificResources.Apis.Count().Should().Be(2);
            apiTemplate.SpecificResources.Apis.All(x => x.Type == ResourceTypeConstants.API).Should().BeTrue();
            apiTemplate.SpecificResources.Apis.All(x => x.Properties is not null).Should().BeTrue();

            // api schemas
            apiTemplate.SpecificResources.ApiSchemas.Count().Should().Be(2);
            apiTemplate.SpecificResources.ApiSchemas.All(x => x.Type == ResourceTypeConstants.APISchema).Should().BeTrue();
            apiTemplate.SpecificResources.ApiSchemas.All(x => x.Properties is not null).Should().BeTrue();

            // diagnostics
            apiTemplate.SpecificResources.Diagnostics.Count().Should().Be(3);
            apiTemplate.SpecificResources.Diagnostics.All(x => x.Type == ResourceTypeConstants.APIServiceDiagnostic || x.Type == ResourceTypeConstants.APIDiagnostic).Should().BeTrue();
            apiTemplate.SpecificResources.Diagnostics.All(x => x.Properties is not null).Should().BeTrue();

            // tags
            apiTemplate.SpecificResources.Tags.Count().Should().Be(4);
            apiTemplate.SpecificResources.Tags.All(x => x.Type == ResourceTypeConstants.ProductTag).Should().BeTrue();

            // api products
            apiTemplate.SpecificResources.ApiProducts.Count().Should().Be(2);
            apiTemplate.SpecificResources.ApiProducts.All(x => x.Type == ResourceTypeConstants.ProductApi).Should().BeTrue();
            apiTemplate.SpecificResources.ApiProducts.All(x => x.Properties is not null).Should().BeTrue();

            // api policies
            apiTemplate.SpecificResources.ApiPolicies.Count().Should().Be(2);
            apiTemplate.SpecificResources.ApiPolicies.All(x => x.Properties is not null).Should().BeTrue();

            // api operations
            apiTemplate.SpecificResources.ApiOperations.Count().Should().Be(2);
            apiTemplate.SpecificResources.ApiOperations.All(x => x.Type == ResourceTypeConstants.APIOperation).Should().BeTrue();
            apiTemplate.SpecificResources.ApiOperations.All(x => x.Properties is not null).Should().BeTrue();

            // api operations policies
            apiTemplate.SpecificResources.ApiOperationsPolicies.Count().Should().Be(2);
            apiTemplate.SpecificResources.ApiOperations.All(x => x.Properties is not null).Should().BeTrue();

            // api operations tags
            apiTemplate.SpecificResources.ApiOperationsPolicies.Count().Should().Be(2);
            apiTemplate.SpecificResources.ApiOperations.All(x => x.Properties is not null).Should().BeTrue();
        }
    }
}
