// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients;
using Xunit;
using Moq;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Utils;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Gateway Apis Extraction")]
    public class GatewayApiExtractorTests : ExtractorMockerWithOutputTestsBase
    {
        public GatewayApiExtractorTests() : base("gateway-apis-tests")
        {
        }

        [Fact]
        public async Task GenerateGatewayApisTemplates_ProperlyLaysTheInformation()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateGatewayApisTemplates_ProperlyLaysTheInformation));

            var extractorConfig = this.GetMockedExtractorConsoleAppConfiguration(
                splitApis: false,
                apiVersionSetName: string.Empty,
                multipleApiNames: string.Empty,
                includeAllRevisions: false);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var mockedGatewayClient = MockGatewayClient.GetMockedApiClientWithDefaultValues();
            var mockedApisClient = MockApisClient.GetMockedApiClientWithDefaultValues();

            var gatewayApiExtractor = new GatewayApiExtractor(
                this.GetTestLogger<GatewayApiExtractor>(),
                new TemplateBuilder(),
                mockedGatewayClient,
                mockedApisClient,
                null);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                gatewayApiExtractor: gatewayApiExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var gatewayApiTemplate = await extractorExecutor.GenerateGatewayApiTemplateAsync(
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.GatewayApi)).Should().BeTrue();

            gatewayApiTemplate.Should().NotBeNull();
            gatewayApiTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            gatewayApiTemplate.TypedResources.GatewayApis.Count().Should().Be(4);
            gatewayApiTemplate.Resources.Count().Should().Be(4);

            var gatewayResources = gatewayApiTemplate.TypedResources.GatewayApis;
            gatewayResources.Where(x => x.Name.Contains(MockGatewayClient.GatewayName1)).Count().Should().Be(2);
            gatewayResources.Where(x => x.Name.Contains(MockGatewayClient.GatewayName2)).Count().Should().Be(2);
            
            gatewayResources.All(x => x.ApiVersion == GlobalConstants.ApiVersion).Should().BeTrue();
            gatewayResources.All(x => x.Type == ResourceTypeConstants.GatewayApi).Should().BeTrue();
            gatewayResources.All(x => x.Properties is not null).Should().BeTrue();
        }

        [Fact]
        public async Task GenerateGatewayApisTemplates_ProperlyLaysTheInformation_GivenApiNameParameterIsGiven()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateGatewayApisTemplates_ProperlyLaysTheInformation_GivenApiNameParameterIsGiven));
            var apiName = "api-name";
            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                extractGateways: "true",
                apiName: apiName);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var gatewayListResponsefileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListGateways_success_response.json");
            var mockedGatewaysClient = await MockGatewayClient.GetMockedHttpGatewaysClient(new MockClientConfiguration(responseFileLocation: gatewayListResponsefileLocation));

            var getSingleApiResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementGetApiContract_success_response.json");
            var gatewayApisListResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListGatewayApis_success_response.json");
            var gatewayApisLisEmptytResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListEmptyValue_success_response.json");
            var mockedApisClient = await MockApisClient.GetMockedHttpApiClient(
                new MockClientConfiguration(responseFileLocation: gatewayApisListResponseFileLocation, urlPath: $"gateways/gw-1/apis?api-version={GlobalConstants.ApiVersion}"),
                new MockClientConfiguration(responseFileLocation: gatewayApisLisEmptytResponseFileLocation, urlPath: $"gateways/gw-2/apis?api-version={GlobalConstants.ApiVersion}"),
                new MockClientConfiguration(responseFileLocation: getSingleApiResponseFileLocation, urlPath: $"apis/{apiName}?api-version={GlobalConstants.ApiVersion}"));
            var mockedApiClientUtils = new ApiClientUtils(mockedApisClient, this.GetTestLogger<ApiClientUtils>());

            var gatewayApiExtractor = new GatewayApiExtractor(
                this.GetTestLogger<GatewayApiExtractor>(),
                new TemplateBuilder(),
                mockedGatewaysClient,
                mockedApisClient,
                mockedApiClientUtils);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                gatewayApiExtractor: gatewayApiExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var gatewayApiTemplate = await extractorExecutor.GenerateGatewayApiTemplateAsync(
                apiName,
                null,
                currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.GatewayApi)).Should().BeTrue();

            gatewayApiTemplate.Should().NotBeNull();
            gatewayApiTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            gatewayApiTemplate.TypedResources.GatewayApis.Count().Should().Be(1);
            gatewayApiTemplate.Resources.Count().Should().Be(1);

            var gatewayResources = gatewayApiTemplate.TypedResources.GatewayApis;
            gatewayResources.Any(x => x.Name.Contains("/gw-1")).Should().BeTrue();

            gatewayResources.All(x => x.ApiVersion == GlobalConstants.ApiVersion).Should().BeTrue();
            gatewayResources.All(x => x.Type == ResourceTypeConstants.GatewayApi).Should().BeTrue();
            gatewayResources.All(x => x.Properties is not null).Should().BeTrue();
        }

        [Fact]
        public async Task GenerateGatewayApisTemplates_ProperlyLaysTheInformation_GivenApiNameParameterIsEmpty()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateGatewayApisTemplates_ProperlyLaysTheInformation_GivenApiNameParameterIsEmpty));
            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                extractGateways: "true",
                apiName: string.Empty);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var gatewayListResponsefileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListGateways_success_response.json");
            var mockedGatewaysClient = await MockGatewayClient.GetMockedHttpGatewaysClient(new MockClientConfiguration(responseFileLocation: gatewayListResponsefileLocation));

            var gatewayApisListResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListGatewayApis_success_response.json");
            var gatewayApisLisEmptytResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListEmptyValue_success_response.json");
            var mockedApisClient = await MockApisClient.GetMockedHttpApiClient(
                new MockClientConfiguration(responseFileLocation: gatewayApisListResponseFileLocation, urlPath: $"gateways/gw-1/apis?api-version={GlobalConstants.ApiVersion}"),
                new MockClientConfiguration(responseFileLocation: gatewayApisLisEmptytResponseFileLocation, urlPath: $"gateways/gw-2/apis?api-version={GlobalConstants.ApiVersion}"));
            var mockedApiClientUtils = new ApiClientUtils(mockedApisClient, this.GetTestLogger<ApiClientUtils>());

            var gatewayApiExtractor = new GatewayApiExtractor(
                this.GetTestLogger<GatewayApiExtractor>(),
                new TemplateBuilder(),
                mockedGatewaysClient,
                mockedApisClient,
                mockedApiClientUtils);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                gatewayApiExtractor: gatewayApiExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var gatewayApiTemplate = await extractorExecutor.GenerateGatewayApiTemplateAsync(
                string.Empty,
                null,
                currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.GatewayApi)).Should().BeTrue();

            gatewayApiTemplate.Should().NotBeNull();
            gatewayApiTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            gatewayApiTemplate.TypedResources.GatewayApis.Count().Should().Be(1);
            gatewayApiTemplate.Resources.Count().Should().Be(1);

            var gatewayResources = gatewayApiTemplate.TypedResources.GatewayApis;
            gatewayResources.Any(x => x.Name.Contains("/gw-1")).Should().BeTrue();

            gatewayResources.All(x => x.ApiVersion == GlobalConstants.ApiVersion).Should().BeTrue();
            gatewayResources.All(x => x.Type == ResourceTypeConstants.GatewayApi).Should().BeTrue();
            gatewayResources.All(x => x.Properties is not null).Should().BeTrue();
        }
    }
}
