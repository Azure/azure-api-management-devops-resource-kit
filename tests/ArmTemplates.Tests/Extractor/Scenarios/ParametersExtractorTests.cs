// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Parameters Extraction")]
    public class ParametersExtractorTests : ExtractorMockerWithOutputTestsBase
    {
        public ParametersExtractorTests() : base("parameters-tests")
        {
        }

        ExtractorExecutor GetExtractorInstance(ExtractorParameters extractorParameters) 
        {
            var parametersExtractor = new ParametersExtractor(new TemplateBuilder(), null);

            var loggerExtractor = new LoggerExtractor(
                this.GetTestLogger<LoggerExtractor>(),
                new TemplateBuilder(),
                null,
                null);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                parametersExtractor: parametersExtractor,
                loggerExtractor: loggerExtractor);

            extractorExecutor.SetExtractorParameters(extractorParameters);

            return extractorExecutor;
        }

        [Fact]
        public async Task GenerateParametersTemplates_ProperlyLaysTheInformation()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateParametersTemplates_ProperlyLaysTheInformation));

            var extractorParameters = this.CreateDefaultExtractorParameters(
                policyXmlBaseUrl: string.Empty,
                policyXmlSasToken: string.Empty
            );

            var extractorExecutor = this.GetExtractorInstance(extractorParameters);

            // act
            var parametersTemplate = await extractorExecutor.GenerateParametersTemplateAsync(null, null, null, null, currentTestDirectory);

            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Parameters)).Should().BeTrue();

            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.PolicyXMLBaseUrl);
            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.PolicyXMLSasToken);

        }

        [Fact]
        public async Task GenerateParametersTemplates_ProperlyLaysTheInformation_PolicyExcluded()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateParametersTemplates_ProperlyLaysTheInformation_PolicyExcluded));

            var extractorParameters = this.CreateDefaultExtractorParameters(
                policyXmlBaseUrl: null,
                policyXmlSasToken: null
            );

            var extractorExecutor = this.GetExtractorInstance(extractorParameters);
            
            // act
            var parametersTemplate = await extractorExecutor.GenerateParametersTemplateAsync(null, null, null, null, currentTestDirectory);

            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Parameters)).Should().BeTrue();

            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            parametersTemplate.Parameters.Should().NotContainKey(ParameterNames.PolicyXMLBaseUrl);
            parametersTemplate.Parameters.Should().NotContainKey(ParameterNames.PolicyXMLSasToken);
        }
    }
}
