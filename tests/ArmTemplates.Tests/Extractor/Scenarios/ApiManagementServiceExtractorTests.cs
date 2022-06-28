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
    }
}
