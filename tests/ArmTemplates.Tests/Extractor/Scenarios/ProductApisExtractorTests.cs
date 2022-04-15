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
            productApisTemplate.TypedResources.ProductApis.Count().Should().Be(2);
            productApisTemplate.Resources.Count().Should().Be(2);

            var productApi1 = productApisTemplate.TypedResources.ProductApis.First();
            productApi1.ApiVersion.Should().Be(GlobalConstants.ApiVersion);
            productApi1.Name.Should().NotBeNullOrEmpty();
            productApi1.Type.Should().Be(ResourceTypeConstants.ProductApi);
            productApi1.Properties.DisplayName.Should().NotBeNullOrEmpty();
            productApi1.Properties.Description.Should().NotBeNullOrEmpty();

            var productApi2 = productApisTemplate.TypedResources.ProductApis.Last();
            productApi2.ApiVersion.Should().Be(GlobalConstants.ApiVersion);
            productApi2.Name.Should().NotBeNullOrEmpty();
            productApi2.Type.Should().Be(ResourceTypeConstants.ProductApi);
            productApi2.Properties.DisplayName.Should().NotBeNullOrEmpty();
            productApi2.Properties.Description.Should().NotBeNullOrEmpty();
            productApi2.DependsOn.Should().NotBeNullOrEmpty();
        }
    }
}
