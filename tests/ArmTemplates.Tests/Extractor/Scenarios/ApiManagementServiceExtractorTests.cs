// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiManagementService;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "ApiManagementService Extraction")]
    public class ApiManagementServiceExtractorTests : ExtractorMockerWithOutputTestsBase
    {
        public ApiManagementServiceExtractorTests() : base("api-management-service")
        {
        }

        [Fact]
        public async Task GenerateApiManagementService_ProperlyLaysTheInformation()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateApiManagementService_ProperlyLaysTheInformation));
            
            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration();
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var mockedApiManagementServiceClient = MockApiManagementServiceClient.GetMockedApiManagementServiceClientWithDefaultValues();
            var mockedApiManagementServiceExtractor = new ApiManagementServiceExtractor(this.GetTestLogger<ApiManagementServiceExtractor>(), new TemplateBuilder(), mockedApiManagementServiceClient);
            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                apiManagementServiceExtractor: mockedApiManagementServiceExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var apiManagementServiceTemplate = await extractorExecutor.GenerateApiManagementServiceTemplate(currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.ApiManagementService)).Should().BeTrue();

            apiManagementServiceTemplate.Parameters.Should().NotBeNull();
            apiManagementServiceTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            apiManagementServiceTemplate.Resources.Count().Should().Be(1);
            apiManagementServiceTemplate.TypedResources.ApiManagementServices.Count().Should().Be(1);
        }

        [Fact]
        public async Task GenerateApiManagementService_ProperlyParsesJsonResponse()
        {
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateApiManagementService_ProperlyParsesJsonResponse));
            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration();
            var extractorParameters = new ExtractorParameters(extractorConfig);

            string fileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementGetInstance_success_response.json");
            var expectedApiManagementService = await MockClientUtils.DeserializeFileContent<ApiManagementServiceResource>(fileLocation);
            var mockedApiManagementServiceClient = await MockApiManagementServiceClient.GetMockedHttpApiManagementServiceClient(new MockClientConfiguration(responseFileLocation: fileLocation));
            var mockedApiManagementServiceExtractor = new ApiManagementServiceExtractor(this.GetTestLogger<ApiManagementServiceExtractor>(), new TemplateBuilder(), mockedApiManagementServiceClient);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                apiManagementServiceExtractor: mockedApiManagementServiceExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var apiManagementServiceTemplate = await extractorExecutor.GenerateApiManagementServiceTemplate(currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.ApiManagementService)).Should().BeTrue();

            apiManagementServiceTemplate.Parameters.Should().NotBeNull();
            apiManagementServiceTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            apiManagementServiceTemplate.Resources.Count().Should().Be(1);
            apiManagementServiceTemplate.TypedResources.ApiManagementServices.Count().Should().Be(1);

            var apiManagementService = apiManagementServiceTemplate.TypedResources.ApiManagementServices[0];

            apiManagementService.Properties.PublisherEmail.Should().Be(expectedApiManagementService.Properties.PublisherEmail);
            apiManagementService.Properties.DisableGateway.Should().Be(expectedApiManagementService.Properties.DisableGateway);
            apiManagementService.Properties.EnableClientCertificate.Should().Be(expectedApiManagementService.Properties.EnableClientCertificate);
            apiManagementService.Properties.NotificationSenderEmail.Should().Be(expectedApiManagementService.Properties.NotificationSenderEmail);
            apiManagementService.Properties.PlatformVersion.Should().Be(expectedApiManagementService.Properties.PlatformVersion);
            apiManagementService.Properties.ProvisioningState.Should().Be(expectedApiManagementService.Properties.ProvisioningState);
            apiManagementService.Properties.PublisherName.Should().Be(expectedApiManagementService.Properties.PublisherName);
            apiManagementService.Properties.TargetProvisioningState.Should().Be(expectedApiManagementService.Properties.TargetProvisioningState);
        }
    }
}
