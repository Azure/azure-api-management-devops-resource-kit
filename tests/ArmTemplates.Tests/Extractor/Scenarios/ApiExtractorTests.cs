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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiOperations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
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

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                multipleAPIs: string.Empty,
                apiVersionSetName: string.Empty,
                includeAllRevisions: "false",
                splitAPIs: "false",
                policyXmlBaseUrl: "policyXmlUrl",
                policyXmlSasToken: "policyXmlSasToken",
                linkedTemplatesBaseUrl: "linkedBaseUrl",
                linkedTemplatesSasToken: "linkedUrlToken",
                apiParameters: new Dictionary<string, ApiParameterProperty> { { "test-service-url-property-api-name", new ApiParameterProperty(null, "test-service-url-property-url") } },
                paramServiceUrl: "true",
                paramNamedValue: "true",
                paramApiLoggerId: "true",
                paramLogResourceId: "true",
                serviceBaseUrl: "test-service-base-url",
                notIncludeNamedValue: "true",
                paramNamedValuesKeyVaultSecrets: "true",
                paramBackend: "true",
                extractGateways: "true",
                paramApiOauth2Scope: "true"
                );

            var extractorParameters = new ExtractorParameters(extractorConfig);

            // mocked clients
            var mockedApiClient = MockApisClient.GetMockedApiClientWithDefaultValues();
            var mockedProductClient = MockProductsClient.GetMockedApiClientWithDefaultValues();
            var mockedApiSchemaClient = MockApiSchemaClient.GetMockedApiClientWithDefaultValues();
            var mockedPolicyClient = MockPolicyClient.GetMockedApiClientWithDefaultValues();
            var mockedTagClient = MockTagClient.GetMockedApiClientWithDefaultValues();
            var mockedApiOperationClient = MockApiOperationClient.GetMockedApiClientWithDefaultValues();
            var mockedDiagnosticClient = MockDiagnosticClient.GetMockedClientWithApiDependentValues();
            var mockedRevisionClient = MockApisRevisionsClient.GetMockedApiRevisionClientWithDefaultValues();

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
                mockedApiOperationExtractor,
                mockedRevisionClient
                );

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
            Directory.GetFiles(Path.Combine(currentTestDirectory, PolicyExtractor.PoliciesDirectoryName)).Count().Should().Be(6);

            apiTemplate.Parameters.Should().NotBeNull();
            apiTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            apiTemplate.Parameters.Should().ContainKey(ParameterNames.ServiceUrl);
            apiTemplate.Parameters.Should().ContainKey(ParameterNames.ApiLoggerId);
            apiTemplate.Parameters.Should().ContainKey(ParameterNames.PolicyXMLBaseUrl);
            apiTemplate.Parameters.Should().ContainKey(ParameterNames.PolicyXMLSasToken);
            apiTemplate.Resources.Count().Should().Be(33);

            // apis
            apiTemplate.TypedResources.Apis.Count().Should().Be(3);
            apiTemplate.TypedResources.Apis.All(x => x.Type == ResourceTypeConstants.API).Should().BeTrue();
            apiTemplate.TypedResources.Apis.All(x => x.Properties is not null).Should().BeTrue();

            // api schemas
            apiTemplate.TypedResources.ApiSchemas.Count().Should().Be(3);
            apiTemplate.TypedResources.ApiSchemas.All(x => x.Type == ResourceTypeConstants.APISchema).Should().BeTrue();
            apiTemplate.TypedResources.ApiSchemas.All(x => x.Properties is not null).Should().BeTrue();

            // diagnostics
            apiTemplate.TypedResources.Diagnostics.Count().Should().Be(4);
            apiTemplate.TypedResources.Diagnostics.All(x => x.Type == ResourceTypeConstants.APIServiceDiagnostic || x.Type == ResourceTypeConstants.APIDiagnostic).Should().BeTrue();
            apiTemplate.TypedResources.Diagnostics.All(x => x.Properties is not null).Should().BeTrue();

            // tags
            apiTemplate.TypedResources.Tags.Count().Should().Be(6);
            apiTemplate.TypedResources.Tags.All(x => x.Type == ResourceTypeConstants.APITag).Should().BeTrue();

            // api products
            apiTemplate.TypedResources.ApiProducts.Count().Should().Be(3);
            apiTemplate.TypedResources.ApiProducts.All(x => x.Type == ResourceTypeConstants.ProductApi).Should().BeTrue();
            apiTemplate.TypedResources.ApiProducts.All(x => x.Properties is not null).Should().BeTrue();

            // api policies
            apiTemplate.TypedResources.ApiPolicies.Count().Should().Be(3);
            apiTemplate.TypedResources.ApiPolicies.All(x => x.Properties is not null).Should().BeTrue();

            // api operations
            apiTemplate.TypedResources.ApiOperations.Count().Should().Be(2);
            apiTemplate.TypedResources.ApiOperations.All(x => x.Type == ResourceTypeConstants.APIOperation).Should().BeTrue();
            apiTemplate.TypedResources.ApiOperations.All(x => x.Properties is not null).Should().BeTrue();
            apiTemplate.TypedResources.ApiOperations.SelectMany(x => x.DependsOn).Any(x => x.Contains($"'{ResourceTypeConstants.API}'")).Should().BeTrue();
            apiTemplate.TypedResources.ApiOperations.SelectMany(x => x.DependsOn).Any(x => x.Contains($"'{ResourceTypeConstants.APIOperation}'")).Should().BeFalse();
            apiTemplate.TypedResources.ApiOperations.All(x => x.Properties.Request.Representations.Count() == 1).Should().BeTrue();
            apiTemplate.TypedResources.ApiOperations.All(x => x.Properties.Responses.Count() == 1).Should().BeTrue();
            apiTemplate.TypedResources.ApiOperations.All(x => x.Properties.Request.Representations.All(o => o.Examples.Count > 0)).Should().BeTrue();
            apiTemplate.TypedResources.ApiOperations.All(x => x.Properties.Request.Representations.All(o => o.Examples.ContainsKey("default"))).Should().BeTrue();

            // api operations policies
            apiTemplate.TypedResources.ApiOperationsPolicies.Count().Should().Be(3);
            apiTemplate.TypedResources.ApiOperations.All(x => x.Properties is not null).Should().BeTrue();

            // api operations tags
            apiTemplate.TypedResources.ApiOperationsTags.Count().Should().Be(6);
            apiTemplate.TypedResources.ApiOperationsTags.All(x => x.Type == ResourceTypeConstants.APIOperationTag).Should().BeTrue();
        }

        [Fact]
        public async Task GenerateGraphQLApiTemplates()
        {
            FileReader fileReader = new FileReader();
            string fileLocation = Path.Combine("Resources", "Schemas", "schema.gql");

            Task<string> fileReadingTask = fileReader.RetrieveFileContentsAsync(fileLocation);

            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateGraphQLApiTemplates));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
               sourceApimName: string.Empty,
               destinationApimName: string.Empty,
               resourceGroup: string.Empty,
               fileFolder: string.Empty,
               apiName: string.Empty);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            // mocked clients
            var mockedApiClient = MockApisClient.GetMockedApiClientWithDefaultValues();
            var mockedProductClient = MockProductsClient.GetMockedApiClientWithDefaultValues();
            var mockedApiSchemaClient = MockApiSchemaClient.GetMockedApiClientWithGraphQLSchemaValues();
            var mockedPolicyClient = MockPolicyClient.GetMockedApiClientWithDefaultValues();
            var mockedTagClient = MockTagClient.GetMockedApiClientWithDefaultValues();
            var mockedApiOperationClient = MockApiOperationClient.GetMockedApiClientWithDefaultValues();
            var mockedDiagnosticClient = MockDiagnosticClient.GetMockedClientWithApiDependentValues();
            var mockedRevisionClient = MockApisRevisionsClient.GetMockedApiRevisionClientWithDefaultValues();

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
                mockedApiOperationExtractor,
                mockedRevisionClient);

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
            
            //schema name
            string schemaContentType = "application/vnd.ms-azure-apim.graphql.schema";

            // api schemas
            apiTemplate.TypedResources.ApiSchemas.Count().Should().Be(3);
            apiTemplate.TypedResources.ApiSchemas.All(x => x.Type == ResourceTypeConstants.APISchema).Should().BeTrue();
            apiTemplate.TypedResources.ApiSchemas.All(x => x.Properties is not null).Should().BeTrue();
            apiTemplate.TypedResources.ApiSchemas.All(x => x.Properties.Document.Value.ToString().Equals(fileReadingTask.Result.ToString())).Should().BeTrue();
            apiTemplate.TypedResources.ApiSchemas.All(x => x.Properties.ContentType.Equals(schemaContentType)).Should().BeTrue();
        }

        [Fact]
        public async Task GenerateApiTemplateAsync_WebsocketApiTypeOperationsNotGenerated()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateApiTemplateAsync_WebsocketApiTypeOperationsNotGenerated));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
               sourceApimName: string.Empty,
               destinationApimName: string.Empty,
               resourceGroup: string.Empty,
               fileFolder: string.Empty,
               apiName: string.Empty);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            // mocked clients
            var mockedApiClient = MockApisClient.GetMockedApiClientWithDefaultValues();
            var mockedProductClient = MockProductsClient.GetMockedApiClientWithDefaultValues();
            var mockedApiSchemaClient = MockApiSchemaClient.GetMockedApiClientWithGraphQLSchemaValues();
            var mockedPolicyClient = MockPolicyClient.GetMockedApiClientWithDefaultValues();
            var mockedTagClient = MockTagClient.GetMockedApiClientWithDefaultValues();
            var mockedApiOperationClient = MockApiOperationClient.GetMockedApiClientWithDefaultValues();
            var mockedDiagnosticClient = MockDiagnosticClient.GetMockedClientWithApiDependentValues();
            var mockedRevisionClient = MockApisRevisionsClient.GetMockedApiRevisionClientWithDefaultValues();

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
                mockedApiOperationExtractor,
                mockedRevisionClient);

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

            // api operation resources
            apiTemplate.TypedResources.ApiOperations.Count().Should().Be(2);
            apiTemplate.TypedResources.ApiOperations.Any(x => x.Name.Contains("websocket-api")).Should().BeFalse();
            apiTemplate.TypedResources.ApiOperationsTags.Count().Should().Be(6);
            apiTemplate.TypedResources.ApiOperationsPolicies.Count().Should().Be(3);
        }

        [Fact]
        public async Task GenerateApiTemplateAsync_ContainsContactAndTermOfService_Information()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateApiTemplateAsync_ContainsContactAndTermOfService_Information));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                multipleAPIs: string.Empty,
                apiVersionSetName: string.Empty,
                apiName: "apiName"
            );

            var extractorParameters = new ExtractorParameters(extractorConfig);

            // mocked clients
            var responseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementGetApiContract_success_response.json");
            var mockedApiClientAllCurrent = await MockApisClient.GetMockedHttpApiClient(new MockClientConfiguration(responseFileLocation: responseFileLocation));

            // mocked extractors
            var mockedDiagnosticExtractor = new Mock<IDiagnosticExtractor>(MockBehavior.Loose);
            var mockedApiSchemaExtractor = new Mock<IApiSchemaExtractor>(MockBehavior.Loose);
            var mockedPolicyExtractor = new Mock<IPolicyExtractor>(MockBehavior.Loose);
            var mockedProductApisExtractor = new Mock<IProductApisExtractor>(MockBehavior.Loose);
            var mockedTagExtractor = new Mock<ITagExtractor>(MockBehavior.Loose);
            var mockedApiOperationExtractor = new Mock<IApiOperationExtractor>(MockBehavior.Loose);
            mockedApiOperationExtractor.Setup(
                    x => x.GenerateApiOperationsResourcesAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>())
                ).ReturnsAsync(new List<ApiOperationTemplateResource>());
                

            var apiExtractor = new ApiExtractor(
                this.GetTestLogger<ApiExtractor>(),
                new TemplateBuilder(),
                mockedApiClientAllCurrent,
                mockedDiagnosticExtractor.Object,
                mockedApiSchemaExtractor.Object,
                mockedPolicyExtractor.Object,
                mockedProductApisExtractor.Object,
                mockedTagExtractor.Object,
                mockedApiOperationExtractor.Object,
                null
                );

            // act
            var apiTemplate = await apiExtractor.GenerateSingleApiTemplateResourcesAsync(
                singleApiName: "api-contract-and-terms",
                currentTestDirectory,
                extractorParameters);

            // assert
            apiTemplate.Apis.Should().NotBeNull();
            apiTemplate.Apis.Count.Should().Be(1);
            apiTemplate.Apis[0].Name.Contains("api-contract-and-terms").Should().BeTrue();
            apiTemplate.Apis[0].Properties.Contact.Should().NotBeNull();
            apiTemplate.Apis[0].Properties.Contact.Name.Should().Be("name-value");
            apiTemplate.Apis[0].Properties.Contact.Email.Should().Be("email-value");
            apiTemplate.Apis[0].Properties.Contact.Url.Should().Be("url-value");
            apiTemplate.Apis[0].Properties.TermsOfServiceUrl.Should().Be("test-url-value");
        }
    }
}
