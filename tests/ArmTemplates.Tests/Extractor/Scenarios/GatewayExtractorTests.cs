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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Utils;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Gateways Extraction")]
    public class GatewayExtractorTests : ExtractorMockerWithOutputTestsBase
    {
        public GatewayExtractorTests() : base("gateways-tests")
        {
        }

        [Fact]
        public async Task GenerateGatewayTemplates_ProperlyLaysTheInformation()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateGatewayTemplates_ProperlyLaysTheInformation));

            var extractorConfig = this.GetMockedExtractorConsoleAppConfiguration(
                splitApis: false,
                apiVersionSetName: string.Empty,
                multipleApiNames: string.Empty,
                includeAllRevisions: false);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var mockedGatewayClient = MockGatewayClient.GetMockedApiClientWithDefaultValues();
            var gatewayExtractor = new GatewayExtractor(
                this.GetTestLogger<GatewayExtractor>(),
                new TemplateBuilder(),
                mockedGatewayClient,
                null);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                gatewayExtractor: gatewayExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var gatewayTemplate = await extractorExecutor.GenerateGatewayTemplateAsync(
                It.IsAny<string>(),
                currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Gateway)).Should().BeTrue();

            gatewayTemplate.Should().NotBeNull();
            gatewayTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            gatewayTemplate.TypedResources.Gateways.Count().Should().Be(2);
            gatewayTemplate.Resources.Count().Should().Be(2);

            var gatewayResources = gatewayTemplate.TypedResources.Gateways;
            gatewayResources.Any(x => x.Name.Contains(MockGatewayClient.GatewayName1)).Should().BeTrue();
            gatewayResources.Any(x => x.Name.Contains(MockGatewayClient.GatewayName2)).Should().BeTrue();
            gatewayResources.All(x => x.ApiVersion == GlobalConstants.ApiVersion).Should().BeTrue();
            gatewayResources.All(x => x.Type == ResourceTypeConstants.Gateway).Should().BeTrue();
            gatewayResources.All(x => x.Properties is not null).Should().BeTrue();
        }

        
        [Fact]
        public async Task GenerateGatewayTemplates_ProperlyLaysTheInformation_GivenApiNameParameterExists()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateGatewayTemplates_ProperlyLaysTheInformation_GivenApiNameParameterExists));
            var apiName = "api-name";
            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                apiName: apiName,
                extractGateways: "true"
            );
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var getSingleApiResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementGetApiContract_success_response.json");
            var gatewayApisListResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListGatewayApis_success_response.json");
            var gatewayApisLisEmptytResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListEmptyValue_success_response.json");
            
            var mockedApisClient = await MockApisClient.GetMockedHttpApiClient(
                new MockClientConfiguration(responseFileLocation: gatewayApisListResponseFileLocation, urlPath: $"gateways/gw-1/apis?api-version={GlobalConstants.ApiVersion}"),
                new MockClientConfiguration(responseFileLocation: gatewayApisLisEmptytResponseFileLocation, urlPath: $"gateways/gw-2/apis?api-version={GlobalConstants.ApiVersion}"),
                new MockClientConfiguration(responseFileLocation: getSingleApiResponseFileLocation, urlPath: $"apis/{apiName}?api-version={GlobalConstants.ApiVersion}"));
            var mockedApiClientUtils = new ApiClientUtils(mockedApisClient, this.GetTestLogger<ApiClientUtils>());

            var gatewayListResponsefileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListGateways_success_response.json");
            var mockedGatewaysClient = await MockGatewayClient.GetMockedHttpGatewaysClient(new MockClientConfiguration(responseFileLocation: gatewayListResponsefileLocation));

            var gatewayExtractor = new GatewayExtractor(
                this.GetTestLogger<GatewayExtractor>(),
                new TemplateBuilder(),
                mockedGatewaysClient,
                mockedApiClientUtils);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                gatewayExtractor: gatewayExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var gatewayTemplate = await extractorExecutor.GenerateGatewayTemplateAsync(
                apiName,
                currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Gateway)).Should().BeTrue();

            gatewayTemplate.Should().NotBeNull();
            gatewayTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            gatewayTemplate.TypedResources.Gateways.Count().Should().Be(1);
            gatewayTemplate.Resources.Count().Should().Be(1);

            var gatewayResources = gatewayTemplate.TypedResources.Gateways;
            gatewayResources.Any(x => x.Name.Contains("/gw-1")).Should().BeTrue();
            gatewayResources.All(x => x.ApiVersion == GlobalConstants.ApiVersion).Should().BeTrue();
            gatewayResources.All(x => x.Type == ResourceTypeConstants.Gateway).Should().BeTrue();
            gatewayResources.All(x => x.Properties is not null).Should().BeTrue();
        }

        [Fact]
        public async Task GenerateGatewayTemplates_ExtractsAllGateways_GivenApiNameParameterIsEmpty()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateGatewayTemplates_ExtractsAllGateways_GivenApiNameParameterIsEmpty));
            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                apiName: string.Empty,
                extractGateways: "true"
            );
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var gatewayListResponsefileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListGateways_success_response.json");
            var mockedGatewaysClient = await MockGatewayClient.GetMockedHttpGatewaysClient(new MockClientConfiguration(responseFileLocation: gatewayListResponsefileLocation));

            var gatewayExtractor = new GatewayExtractor(
                this.GetTestLogger<GatewayExtractor>(),
                new TemplateBuilder(),
                mockedGatewaysClient,
                null);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                gatewayExtractor: gatewayExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var gatewayTemplate = await extractorExecutor.GenerateGatewayTemplateAsync(
                string.Empty,
                currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Gateway)).Should().BeTrue();

            gatewayTemplate.Should().NotBeNull();
            gatewayTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            gatewayTemplate.TypedResources.Gateways.Count().Should().Be(2);
            gatewayTemplate.Resources.Count().Should().Be(2);

            var gatewayResources = gatewayTemplate.TypedResources.Gateways;
            gatewayResources.Any(x => x.Name.Contains("/gw-1")).Should().BeTrue();
            gatewayResources.Any(x => x.Name.Contains("/gw-2")).Should().BeTrue();
            gatewayResources.All(x => x.ApiVersion == GlobalConstants.ApiVersion).Should().BeTrue();
            gatewayResources.All(x => x.Type == ResourceTypeConstants.Gateway).Should().BeTrue();
            gatewayResources.All(x => x.Properties is not null).Should().BeTrue();
        }
    }
}
