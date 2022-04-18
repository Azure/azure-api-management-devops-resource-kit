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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients;
using Moq;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Loggers Extraction")]
    public class LoggerExtractorTests : ExtractorMockerWithOutputTestsBase
    {        
        public LoggerExtractorTests() : base("loggers-tests")
        {
        }

        [Fact]
        public async Task GenerateLoggersTemplates_ProperlyLaysTheInformation()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateLoggersTemplates_ProperlyLaysTheInformation));

            var extractorConfig = this.GetMockedExtractorConsoleAppConfiguration(
                splitApis: false,
                apiVersionSetName: string.Empty,
                multipleApiNames: string.Empty,
                includeAllRevisions: false);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var mockedLoggerClient = MockLoggerClient.GetMockedClientWithDiagnosticDependentValues();
            var mockedDiagnosticClient = MockDiagnosticClient.GetMockedApiClientWithDefaultValues();
            var loggerExtractor = new LoggerExtractor(
                this.GetTestLogger<LoggerExtractor>(), 
                new TemplateBuilder(),
                mockedLoggerClient,
                mockedDiagnosticClient);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                loggerExtractor: loggerExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var loggerTemplate = await extractorExecutor.GenerateLoggerTemplateAsync(
                new List<string> { MockApiName },
                It.IsAny<List<PolicyTemplateResource>>(),
                currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Loggers)).Should().BeTrue();

            loggerTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            loggerTemplate.TypedResources.Loggers.Should().HaveCount(1);
            loggerTemplate.Resources.Should().NotBeNullOrEmpty();

            loggerTemplate.TypedResources.Loggers.First().Name.Should().Contain(MockLoggerClient.LoggerName);
            loggerTemplate.TypedResources.Loggers.First().Properties.LoggerType.Should().Be(MockDiagnosticClient.DefaultDiagnosticName);
        }
    }
}
