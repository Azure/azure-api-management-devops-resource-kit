// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.IO;
using System.Linq;
using System.Net.Http;
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
    [Trait("Category", "Products Extraction")]
    public class ProductsExtractorTests : ExtractorMockerWithOutputTestsBase
    {
        public ProductsExtractorTests() : base("products-tests")
        {
        }

        [Fact]
        public async Task GenerateProductsTemplates_ProperlyLaysTheInformation()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateProductsTemplates_ProperlyLaysTheInformation));

            var extractorConfig = this.GetMockedExtractorConsoleAppConfiguration(
                splitApis: false,
                apiVersionSetName: string.Empty,
                multipleApiNames: string.Empty,
                includeAllRevisions: false);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var mockedPolicyClient = MockPolicyClient.GetMockedApiClientWithDefaultValues();
            var mockedPolicyExtractor = new PolicyExtractor(this.GetTestLogger<PolicyExtractor>(), mockedPolicyClient, new TemplateBuilder());

            var mockedApisClient = MockApisClient.GetMockedApiClientWithDefaultValues();
            var mockedProductsClient = MockProductsClient.GetMockedApiClientWithDefaultValues();
            var mockedGroupsClient = MockGroupsClient.GetMockedApiClientWithDefaultValues();
            var mockedTagClient = MockTagClient.GetMockedApiClientWithDefaultValues();

            var productExtractor = new ProductExtractor(
                this.GetTestLogger<ProductExtractor>(), 
                mockedPolicyExtractor,
                mockedProductsClient,
                mockedGroupsClient,
                mockedTagClient,
                new TemplateBuilder());

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                productExtractor: productExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var productTemplate = await extractorExecutor.GenerateProductsTemplateAsync(
                singleApiName: It.IsAny<string>(),
                currentTestDirectory);

            // assert

            // generated product policy files
            var policyFileName = string.Format(PolicyExtractor.ProductPolicyFileNameFormat, MockProductsClient.ProductName1);
            File.Exists(Path.Combine(currentTestDirectory, PolicyExtractor.PoliciesDirectoryName, policyFileName)).Should().BeTrue();

            // generated product template files
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Products)).Should().BeTrue();

            var templateParameters = productTemplate.Parameters;
            templateParameters.Should().ContainKey(ParameterNames.ApimServiceName);
            templateParameters.Should().ContainKey(ParameterNames.PolicyXMLBaseUrl);
            templateParameters.Should().ContainKey(ParameterNames.PolicyXMLSasToken);

            var templateResources = productTemplate.Resources;

            // product resource
            var productResource = templateResources.First(x => x.Type == ResourceTypeConstants.Product);
            productResource.Name.Should().Contain(MockProductsClient.ProductName1);

            // group resources
            var groupResources = templateResources.Where(x => x.Type == ResourceTypeConstants.ProductGroup).ToList();
            groupResources.Should().HaveCount(2);
            (groupResources[0].Name.Contains(MockGroupsClient.GroupName1) || groupResources[1].Name.Contains(MockGroupsClient.GroupName1)).Should().BeTrue();
            (groupResources[0].Name.Contains(MockGroupsClient.GroupName2) || groupResources[1].Name.Contains(MockGroupsClient.GroupName2)).Should().BeTrue();

            // policy resources
            var policyResource = templateResources.First(x => x.Type == ResourceTypeConstants.ProductPolicy);
            policyResource.Name.Should().Contain(MockProductsClient.ProductName1);

            // tag resources
            var tagResources = templateResources.Where(x => x.Type == ResourceTypeConstants.ProductTag).ToList();
            tagResources.Should().HaveCount(2);
            (tagResources[0].Name.Contains(MockTagClient.TagName1) || tagResources[1].Name.Contains(MockTagClient.TagName1)).Should().BeTrue();
            (tagResources[0].Name.Contains(MockTagClient.TagName2) || tagResources[1].Name.Contains(MockTagClient.TagName2)).Should().BeTrue();
        }

        [Fact]
        public async Task GenerateProductsTemplates_GeneratesTemplatesCorrectly_GivenApiNameParameterProvided()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateProductsTemplates_GeneratesTemplatesCorrectly_GivenApiNameParameterProvided));
            var apiName = "api-name";
            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                apiName: apiName
            );
            var extractorParameters = new ExtractorParameters(extractorConfig);
            
            var getRelatedProductsResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListApiProducts_success_response.json");
            var getAlldProductsResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListProducts_success_response.json");
            var mockedProductsClient = await MockProductsClient.GetMockedHttpProductClient(
                new MockClientConfiguration(responseFileLocation: getRelatedProductsResponseFileLocation, urlPath: $"apis/{apiName}/products?api-version={GlobalConstants.ApiVersion}"),
                new MockClientConfiguration(responseFileLocation: getAlldProductsResponseFileLocation, urlPath: $"{MockSourceApimName}/products?api-version={GlobalConstants.ApiVersion}")
            );
            
            //default values
            var mockedGroupsClient = MockGroupsClient.GetMockedApiClientWithEmptyValues();
            var mockedTagClient = MockTagClient.GetMockedApiClientWithEmptytValues();

            var mockedPolicyClient = MockPolicyClient.GetMockedApiClientWithEmptyValues();
            var mockedPolicyExtractor = new PolicyExtractor(this.GetTestLogger<PolicyExtractor>(), mockedPolicyClient, new TemplateBuilder());

            var productExtractor = new ProductExtractor(
                this.GetTestLogger<ProductExtractor>(),
                mockedPolicyExtractor,
                mockedProductsClient,
                mockedGroupsClient,
                mockedTagClient,
                new TemplateBuilder());

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                productExtractor: productExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var productTemplate = await extractorExecutor.GenerateProductsTemplateAsync(
                singleApiName: apiName,
                currentTestDirectory);

            // assert
            // generated product template files
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Products)).Should().BeTrue();

            var templateParameters = productTemplate.Parameters;
            templateParameters.Should().ContainKey(ParameterNames.ApimServiceName);

            var templateResources = productTemplate.Resources;
            // product resource
            var productResources = templateResources.Where(x => x.Type == ResourceTypeConstants.Product).ToList();
            productResources.Count.Should().Be(1);
            productResources.Any(x => x.OriginalName == "unlimited").Should().BeTrue();
        }

        [Fact]
        public async Task GenerateProductsTemplates_GeneratesTemplatesCorrectly_GivenApiNameParameterIsEmpty()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateProductsTemplates_GeneratesTemplatesCorrectly_GivenApiNameParameterIsEmpty));
            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                apiName: null
            );
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var getAlldProductsResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListProducts_success_response.json");
            var mockedProductsClient = await MockProductsClient.GetMockedHttpProductClient(
                new MockClientConfiguration(responseFileLocation: getAlldProductsResponseFileLocation, urlPath: $"{MockSourceApimName}/products?api-version={GlobalConstants.ApiVersion}")
            );

            //default values
            var mockedGroupsClient = MockGroupsClient.GetMockedApiClientWithEmptyValues();
            var mockedTagClient = MockTagClient.GetMockedApiClientWithEmptytValues();

            var mockedPolicyClient = MockPolicyClient.GetMockedApiClientWithEmptyValues();
            var mockedPolicyExtractor = new PolicyExtractor(this.GetTestLogger<PolicyExtractor>(), mockedPolicyClient, new TemplateBuilder());

            var productExtractor = new ProductExtractor(
                this.GetTestLogger<ProductExtractor>(),
                mockedPolicyExtractor,
                mockedProductsClient,
                mockedGroupsClient,
                mockedTagClient,
                new TemplateBuilder());

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                productExtractor: productExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var productTemplate = await extractorExecutor.GenerateProductsTemplateAsync(
                singleApiName: null,
                currentTestDirectory);

            // assert
            // generated product template files
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Products)).Should().BeTrue();

            var templateParameters = productTemplate.Parameters;
            templateParameters.Should().ContainKey(ParameterNames.ApimServiceName);

            var templateResources = productTemplate.Resources;
            // product resource
            var productResources = templateResources.Where(x => x.Type == ResourceTypeConstants.Product).ToList();
            productResources.Count.Should().Be(2);
            productResources.Any(x => x.OriginalName == "unlimited").Should().BeTrue();
            productResources.Any(x => x.OriginalName == "starter").Should().BeTrue();
        }

        //new one
        /*[Fact]
        public async Task GenerateGroupsTemplates_DoesNotGenerateTemplate_GivenGroupListMethod_IsNotAllowed()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateGroupsTemplates_DoesNotGenerateTemplate_GivenGroupListMethod_IsNotAllowed));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                apiName: null);
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var fileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagement_methodnotallowed_response.json");
            var mockedGroupsClient = await MockGroupsClient.GetMockedHttpGroupClient(new MockClientConfiguration(responseFileLocation: fileLocation, statusCode: System.Net.HttpStatusCode.BadRequest));
            var groupExtractor = new GroupExtractor(this.GetTestLogger<GroupExtractor>(), new TemplateBuilder(), mockedGroupsClient);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                groupExtractor: groupExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var groupTemplate = await extractorExecutor.GenerateGroupsTemplateAsync(currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Groups)).Should().BeFalse();
        }

        [Fact]
        public async Task GenerateGroupsTemplates_RaisesError_GivenGroupListMethod_ReturnsNotFound()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateGroupsTemplates_RaisesError_GivenGroupListMethod_ReturnsNotFound));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                apiName: null);
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var fileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagement_notfound_response.json");
            var mockedGroupsClient = await MockGroupsClient.GetMockedHttpGroupClient(new MockClientConfiguration(responseFileLocation: fileLocation, statusCode: System.Net.HttpStatusCode.NotFound));
            var groupExtractor = new GroupExtractor(this.GetTestLogger<GroupExtractor>(), new TemplateBuilder(), mockedGroupsClient);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                groupExtractor: groupExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act & assert
            var asyncFunction = async () =>
            {
                await extractorExecutor.GenerateGroupsTemplateAsync(currentTestDirectory);
            };

            var results = await asyncFunction.Should().ThrowAsync<HttpRequestException>().Where(e => e.Message.Contains("404"));
        }*/
    }
}
