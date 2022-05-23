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
using System.Collections.Generic;
using Xunit;
using System.Linq;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Parameters Extraction")]
    public class ParametersExtractorTests : ExtractorMockerWithOutputTestsBase
    {
        public ParametersExtractorTests() : base("parameters-tests")
        {
        }

        ExtractorExecutor GetExtractorInstance(ExtractorParameters extractorParameters, IApisClient apisClient = null) 
        {
            var parametersExtractor = new ParametersExtractor(new TemplateBuilder(), apisClient);

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

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                policyXmlBaseUrl: string.Empty,
                policyXmlSasToken: string.Empty
            );
            var extractorParameters = new ExtractorParameters(extractorConfig);

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

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                policyXmlBaseUrl: null,
                policyXmlSasToken: null
            );
            var extractorParameters = new ExtractorParameters(extractorConfig);


            var extractorExecutor = this.GetExtractorInstance(extractorParameters);
            
            // act
            var parametersTemplate = await extractorExecutor.GenerateParametersTemplateAsync(null, null, null, null, currentTestDirectory);

            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Parameters)).Should().BeTrue();

            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            parametersTemplate.Parameters.Should().NotContainKey(ParameterNames.PolicyXMLBaseUrl);
            parametersTemplate.Parameters.Should().NotContainKey(ParameterNames.PolicyXMLSasToken);
        }

        [Fact]
        public async Task GenerateParametersTemplates_ProperlyLaysTheInformation_ApiOauth2ScopeIncludedWSettings()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateParametersTemplates_ProperlyLaysTheInformation_ApiOauth2ScopeIncludedWSettings));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                parametrizeApiOauth2Scope: "true",
                apiOauth2ScopeParameters: new ApiOauth2ScopeProperty[1] { 
                    new ApiOauth2ScopeProperty("api-name-2", "scope_value2")
                }
            );
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var extractorExecutor = this.GetExtractorInstance(extractorParameters, apisClient: MockApisClient.GetMockedApiClientWithDefaultValues());

            // act
            var parametersTemplate = await extractorExecutor.GenerateParametersTemplateAsync(new List<string>{ "api-name-1", "api-name-2" }, null, null, null, currentTestDirectory);

            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Parameters)).Should().BeTrue();

            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.ApiOauth2ScopeSettings);
            var apiOauth2ScopeSettings = (TemplateObjectParameterProperties)parametersTemplate.Parameters[ParameterNames.ApiOauth2ScopeSettings];
            var apiOauth2ScopeSettingsValue = (Dictionary<string, string>) apiOauth2ScopeSettings.Value;

            apiOauth2ScopeSettingsValue.Count.Should().Be(1);
            apiOauth2ScopeSettingsValue.Should().ContainKey("apiname2");
            apiOauth2ScopeSettingsValue["apiname2"].Should().BeEquivalentTo("scope_value2");
        }

        [Fact]
        public async Task GenerateParametersTemplates_ProperlyLaysTheInformation_ApiOauth2ScopeIncludedWOSettings()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateParametersTemplates_ProperlyLaysTheInformation_ApiOauth2ScopeIncludedWOSettings));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                parametrizeApiOauth2Scope: "true"
            );
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var mockedApiClient = MockApisClient.GetMockedApiClientWithDefaultValues();
            var extractorExecutor = this.GetExtractorInstance(extractorParameters, apisClient: mockedApiClient);

            // act
            var parametersTemplate = await extractorExecutor.GenerateParametersTemplateAsync(new List<string> { "api-name-1", "api-name-2" }, null, null, null, currentTestDirectory);

            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Parameters)).Should().BeTrue();

            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.ApiOauth2ScopeSettings);
            var apiOauth2ScopeSettings = (TemplateObjectParameterProperties)parametersTemplate.Parameters[ParameterNames.ApiOauth2ScopeSettings];
            var apiOauth2ScopeSettingsValue = (Dictionary<string, string>)apiOauth2ScopeSettings.Value;

            apiOauth2ScopeSettingsValue.Count.Should().Be(1);
            apiOauth2ScopeSettingsValue.Should().ContainKey("apiname2");
            apiOauth2ScopeSettingsValue["apiname2"].Should().BeEquivalentTo("scope-default-value-2");
        }
    }
}
