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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Utils;
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
    [Trait("Category", "Product Apis Extraction")]
    public class ProductApisExtractorTests : ExtractorMockerWithOutputTestsBase
    {
        public ProductApisExtractorTests() : base("product-apis-tests")
        {
        }

        [Fact]
        public async Task GenerateProductApisTemplates_ProperlyLaysTheInformation()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateProductApisTemplates_ProperlyLaysTheInformation));

            var extractorConfig = this.GetMockedExtractorConsoleAppConfiguration(
                splitApis: false,
                apiVersionSetName: string.Empty,
                multipleApiNames: string.Empty,
                includeAllRevisions: false);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var mockedServiceApisApiClient = MockApisClient.GetMockedApiClientWithDefaultValues();
            var mockedServiceApisProductsApiClient = MockProductsClient.GetMockedApiClientWithDefaultValues();

            var productApisExtractor = new ProductApisExtractor(
                this.GetTestLogger<ProductApisExtractor>(), 
                mockedServiceApisProductsApiClient,
                mockedServiceApisApiClient,
                new TemplateBuilder());

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                productApisExtractor: productApisExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var productApisTemplate = await extractorExecutor.GenerateProductApisTemplateAsync(
                singleApiName: It.IsAny<string>(), 
                multipleApiNames: It.IsAny<List<string>>(),
                currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.ProductAPIs)).Should().BeTrue();

            productApisTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            productApisTemplate.TypedResources.ProductApis.Count().Should().Be(3);
            productApisTemplate.Resources.Count().Should().Be(3);

            foreach (var productApi in productApisTemplate.TypedResources.ProductApis)
            {
                productApi.ApiVersion.Should().Be(GlobalConstants.ApiVersion);
                productApi.Name.Should().NotBeNullOrEmpty();
                productApi.Type.Should().Be(ResourceTypeConstants.ProductApi);
                productApi.Properties.DisplayName.Should().NotBeNullOrEmpty();
                productApi.Properties.Description.Should().NotBeNullOrEmpty();
                productApi.DependsOn.Should().BeNullOrEmpty();
            }
        }

        [Fact]
        public async Task GenerateProductApisTemplates_GeneratesAllRelatedProductApis_GivenApiNameParameterProvided()
        {
            // arrange
            var apiName = "api-name";
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateProductApisTemplates_GeneratesAllRelatedProductApis_GivenApiNameParameterProvided));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(apiName: apiName);
            var extractorParameters = new ExtractorParameters(extractorConfig);
            
            var getSingleApiResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementGetApiContract_success_response.json");
            var getRelatedProductsResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListApiProducts_success_response.json");
            var mockedApisClient = await MockApisClient.GetMockedHttpApiClient(
                new MockClientConfiguration(responseFileLocation: getSingleApiResponseFileLocation, urlPath: $"apis/{apiName}?api-version={GlobalConstants.ApiVersion}"));
            var mockedApiClientUtils = new ApiClientUtils(mockedApisClient, this.GetTestLogger<ApiClientUtils>());

            var mockedServiceApisProductsApiClient = await MockProductsClient.GetMockedHttpProductClient(
                new MockClientConfiguration(responseFileLocation: getRelatedProductsResponseFileLocation)
            );

            var productApisExtractor = new ProductApisExtractor(
                this.GetTestLogger<ProductApisExtractor>(),
                mockedServiceApisProductsApiClient,
                mockedApisClient,
                new TemplateBuilder());

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                productApisExtractor: productApisExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var productApisTemplate = await extractorExecutor.GenerateProductApisTemplateAsync(
                singleApiName: apiName,
                multipleApiNames: It.IsAny<List<string>>(),
                currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.ProductAPIs)).Should().BeTrue();

            productApisTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            productApisTemplate.TypedResources.ProductApis.Count().Should().Be(1);
            productApisTemplate.Resources.Count().Should().Be(1);

            foreach (var productApi in productApisTemplate.TypedResources.ProductApis)
            {
                productApi.ApiVersion.Should().Be(GlobalConstants.ApiVersion);
                productApi.Name.Should().NotBeNullOrEmpty();
                productApi.Name.Contains($"/{apiName}").Should().BeTrue();
                productApi.Type.Should().Be(ResourceTypeConstants.ProductApi);
                productApi.Properties.DisplayName.Should().NotBeNullOrEmpty();
                productApi.Properties.Description.Should().NotBeNullOrEmpty();
                productApi.DependsOn.Should().BeNullOrEmpty();
            }
        }

        [Fact]
        public async Task GenerateProductApisTemplates_GeneratesAllRelatedProductApis_GivenAllApisAreExtracted()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateProductApisTemplates_GeneratesAllRelatedProductApis_GivenAllApisAreExtracted));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(apiName: string.Empty);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var getAllApisResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListApis_success_response.json");
            var getRelatedProductsResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListApiProducts_success_response.json");
            var mockedApisClient = await MockApisClient.GetMockedHttpApiClient(
                new MockClientConfiguration(responseFileLocation: getAllApisResponseFileLocation));
            var mockedApiClientUtils = new ApiClientUtils(mockedApisClient, this.GetTestLogger<ApiClientUtils>());

            var mockedServiceApisProductsApiClient = await MockProductsClient.GetMockedHttpProductClient(
                new MockClientConfiguration(responseFileLocation: getRelatedProductsResponseFileLocation)
            );

            var productApisExtractor = new ProductApisExtractor(
                this.GetTestLogger<ProductApisExtractor>(),
                mockedServiceApisProductsApiClient,
                mockedApisClient,
                new TemplateBuilder());

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                productApisExtractor: productApisExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var productApisTemplate = await extractorExecutor.GenerateProductApisTemplateAsync(
                singleApiName: null,
                multipleApiNames: It.IsAny<List<string>>(),
                currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.ProductAPIs)).Should().BeTrue();

            productApisTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            productApisTemplate.TypedResources.ProductApis.Count().Should().Be(4);
            productApisTemplate.Resources.Count().Should().Be(4);

            foreach (var productApi in productApisTemplate.TypedResources.ProductApis)
            {
                productApi.ApiVersion.Should().Be(GlobalConstants.ApiVersion);
                productApi.Name.Should().NotBeNullOrEmpty();
                productApi.Type.Should().Be(ResourceTypeConstants.ProductApi);
                productApi.Properties.DisplayName.Should().NotBeNullOrEmpty();
                productApi.Properties.Description.Should().NotBeNullOrEmpty();
                productApi.DependsOn.Should().BeNullOrEmpty();
            }
        }
    }
}
