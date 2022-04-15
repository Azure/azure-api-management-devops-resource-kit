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
                mockedApisClient);

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
    }
}
