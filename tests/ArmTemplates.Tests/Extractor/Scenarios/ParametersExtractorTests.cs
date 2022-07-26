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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.IdentityProviders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.IdentityProviderClients;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.OpenIdConnectProviders;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Parameters Extraction")]
    public class ParametersExtractorTests : ExtractorMockerWithOutputTestsBase
    {
        public ParametersExtractorTests() : base("parameters-tests")
        {
        }

        ExtractorExecutor GetExtractorInstance(ExtractorParameters extractorParameters, IApisClient apisClient = null, IIdentityProviderClient identityProviderClient = null, IOpenIdConnectProvidersClient openIdConnectProviderClient = null) 
        {
            var parametersExtractor = new ParametersExtractor(new TemplateBuilder(), apisClient, identityProviderClient, openIdConnectProviderClient);

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
            var parametersTemplate = await extractorExecutor.GenerateParametersTemplateAsync(null, null, null, null, new IdentityProviderResources(), new OpenIdConnectProviderResources(), currentTestDirectory);

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
            var parametersTemplate = await extractorExecutor.GenerateParametersTemplateAsync(null, null, null, null, new IdentityProviderResources(), new OpenIdConnectProviderResources(), currentTestDirectory);

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
                paramApiOauth2Scope: "true",
                apiParameters: new Dictionary<string, ApiParameterProperty> { { "api-name-2", new ApiParameterProperty(oauth2Scope: "scope_value2", serviceUrl: null) } }
            );
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var extractorExecutor = this.GetExtractorInstance(extractorParameters, apisClient: MockApisClient.GetMockedApiClientWithDefaultValues());

            // act
            var parametersTemplate = await extractorExecutor.GenerateParametersTemplateAsync(new List<string>{ "api-name-1", "api-name-2" }, null, null, null, new IdentityProviderResources(), new OpenIdConnectProviderResources(), currentTestDirectory);

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
                paramApiOauth2Scope: "true"
            );
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var mockedApiClient = MockApisClient.GetMockedApiClientWithDefaultValues();
            var extractorExecutor = this.GetExtractorInstance(extractorParameters, apisClient: mockedApiClient);

            // act
            var parametersTemplate = await extractorExecutor.GenerateParametersTemplateAsync(new List<string> { "api-name-1", "api-name-2" }, null, null, null, new IdentityProviderResources(), new OpenIdConnectProviderResources(), currentTestDirectory);

            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Parameters)).Should().BeTrue();

            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.ApiOauth2ScopeSettings);
            var apiOauth2ScopeSettings = (TemplateObjectParameterProperties)parametersTemplate.Parameters[ParameterNames.ApiOauth2ScopeSettings];
            var apiOauth2ScopeSettingsValue = (Dictionary<string, string>)apiOauth2ScopeSettings.Value;

            apiOauth2ScopeSettingsValue.Count.Should().Be(1);
            apiOauth2ScopeSettingsValue.Should().ContainKey("apiname2");
            apiOauth2ScopeSettingsValue["apiname2"].Should().BeEquivalentTo("scope-default-value-2");
        }

        [Fact]
        public async Task GenerateParametersTemplates_ProperlyLaysTheInformation_IdentityProviderSecrets_EmptyValues()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateParametersTemplates_ProperlyLaysTheInformation_IdentityProviderSecrets_EmptyValues));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration();
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var extractorExecutor = this.GetExtractorInstance(extractorParameters, null);

            var identityProviderResources = new IdentityProviderResources();
            identityProviderResources.IdentityProviders.Add(new IdentityProviderResource() {
                OriginalName = "originalName"
            });

            // act
            var parametersTemplate = await extractorExecutor.GenerateParametersTemplateAsync(null, null, null, null, identityProviderResources, new OpenIdConnectProviderResources(), currentTestDirectory);

            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Parameters)).Should().BeTrue();

            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.SecretValues);
            var secretValuesParameters = (TemplateObjectParameterProperties)parametersTemplate.Parameters[ParameterNames.SecretValues];
            var secretValueParameterValues = (Dictionary<string, Dictionary<string, string>>)secretValuesParameters.Value;

            secretValueParameterValues.Count.Should().Be(1);
            secretValueParameterValues.Should().ContainKey(ParameterNames.IdentityProvidersSecretValues);
            secretValueParameterValues[ParameterNames.IdentityProvidersSecretValues].Should().ContainKey("originalName");
            secretValueParameterValues[ParameterNames.IdentityProvidersSecretValues]["originalName"].Should().Be(string.Empty);
        }

        [Fact]
        public async Task GenerateParametersTemplates_ProperlyLaysTheInformation_IdentityProviderSecrets_FilledValues()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateParametersTemplates_ProperlyLaysTheInformation_IdentityProviderSecrets_FilledValues));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(extractSecrets: "true");
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var identityProviderMockClient = MockIdentityProviderClient.GetMockedIdentityProviderClient(new List<string>()
            {
                "originalName"
            });
            var extractorExecutor = this.GetExtractorInstance(extractorParameters, null, identityProviderClient: identityProviderMockClient);

            var identityProviderResources = new IdentityProviderResources();
            identityProviderResources.IdentityProviders.Add(new IdentityProviderResource()
            {
                OriginalName = "originalName"
            });

            // act
            var parametersTemplate = await extractorExecutor.GenerateParametersTemplateAsync(null, null, null, null, identityProviderResources, new OpenIdConnectProviderResources(), currentTestDirectory);

            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Parameters)).Should().BeTrue();

            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.SecretValues);
            var secretValuesParameters = (TemplateObjectParameterProperties)parametersTemplate.Parameters[ParameterNames.SecretValues];
            var secretValueParameterValues = (Dictionary<string, Dictionary<string, string>>)secretValuesParameters.Value;

            secretValueParameterValues.Count.Should().Be(1);
            secretValueParameterValues.Should().ContainKey(ParameterNames.IdentityProvidersSecretValues);
            secretValueParameterValues[ParameterNames.IdentityProvidersSecretValues].Should().ContainKey("originalName");
            secretValueParameterValues[ParameterNames.IdentityProvidersSecretValues]["originalName"].Should().Be(MockIdentityProviderClient.ClientSecretDefaultValue);
        }

        [Fact]
        public async Task GenerateParametersTemplates_ProperlyLaysTheInformation_OpenIdConnectProviderSecrets_EmptyValues()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateParametersTemplates_ProperlyLaysTheInformation_OpenIdConnectProviderSecrets_EmptyValues));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration();
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var extractorExecutor = this.GetExtractorInstance(extractorParameters, null);

            var openIdConnectProviderResources = new OpenIdConnectProviderResources();
            openIdConnectProviderResources.OpenIdConnectProviders.Add(new OpenIdConnectProviderResource()
            {
                OriginalName = "originalName"
            });

            // act
            var parametersTemplate = await extractorExecutor.GenerateParametersTemplateAsync(null, null, null, null, new IdentityProviderResources(), openIdConnectProviderResources, currentTestDirectory);

            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Parameters)).Should().BeTrue();

            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.SecretValues);
            var secretValuesParameters = (TemplateObjectParameterProperties)parametersTemplate.Parameters[ParameterNames.SecretValues];
            var secretValueParameterValues = (Dictionary<string, Dictionary<string, string>>)secretValuesParameters.Value;

            secretValueParameterValues.Count.Should().Be(1);
            secretValueParameterValues.Should().ContainKey(ParameterNames.OpenIdConnectProvidersSecretValues);
            secretValueParameterValues[ParameterNames.OpenIdConnectProvidersSecretValues].Should().ContainKey("originalName");
            secretValueParameterValues[ParameterNames.OpenIdConnectProvidersSecretValues]["originalName"].Should().Be(string.Empty);
        }

        [Fact]
        public async Task GenerateParametersTemplates_ProperlyLaysTheInformation_OpenIdConnectProviderSecrets_FilledValues()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateParametersTemplates_ProperlyLaysTheInformation_OpenIdConnectProviderSecrets_FilledValues));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(extractSecrets: "true");
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var openIdConnectProviderMockClient = MockOpenIdConnectProviderClient.GetMockedOpenIdConnectProviderClient(new List<string>()
            {
                "originalName"
            });
            var extractorExecutor = this.GetExtractorInstance(extractorParameters, null, openIdConnectProviderClient: openIdConnectProviderMockClient);

            var openIdConnectProviderResources = new OpenIdConnectProviderResources();
            openIdConnectProviderResources.OpenIdConnectProviders.Add(new OpenIdConnectProviderResource()
            {
                OriginalName = "originalName"
            });

            // act
            var parametersTemplate = await extractorExecutor.GenerateParametersTemplateAsync(null, null, null, null, new IdentityProviderResources(), openIdConnectProviderResources, currentTestDirectory);

            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Parameters)).Should().BeTrue();

            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.SecretValues);
            var secretValuesParameters = (TemplateObjectParameterProperties)parametersTemplate.Parameters[ParameterNames.SecretValues];
            var secretValueParameterValues = (Dictionary<string, Dictionary<string, string>>)secretValuesParameters.Value;

            secretValueParameterValues.Count.Should().Be(1);
            secretValueParameterValues.Should().ContainKey(ParameterNames.OpenIdConnectProvidersSecretValues);
            secretValueParameterValues[ParameterNames.OpenIdConnectProvidersSecretValues].Should().ContainKey("originalName");
            secretValueParameterValues[ParameterNames.OpenIdConnectProvidersSecretValues]["originalName"].Should().Be(MockOpenIdConnectProviderClient.ClientSecretDefaultValue);
        }

        [Fact]
        public async Task GenerateParametersTemplates_ProperlyLaysTheInformation_ProviderSecrets_GeneratedForOpenIdConnect_Identity_Providers()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateParametersTemplates_ProperlyLaysTheInformation_ProviderSecrets_GeneratedForOpenIdConnect_Identity_Providers));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration();
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var extractorExecutor = this.GetExtractorInstance(extractorParameters, null);

            var openIdConnectProviderResources = new OpenIdConnectProviderResources();
            openIdConnectProviderResources.OpenIdConnectProviders.Add(new OpenIdConnectProviderResource()
            {
                OriginalName = "originalNameOpenIdConnectProvider"
            });

            var identityProviderResources = new IdentityProviderResources();
            identityProviderResources.IdentityProviders.Add(new IdentityProviderResource()
            {
                OriginalName = "originalNameIdentityProvider"
            });

            // act
            var parametersTemplate = await extractorExecutor.GenerateParametersTemplateAsync(null, null, null, null, identityProviderResources, openIdConnectProviderResources, currentTestDirectory);

            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Parameters)).Should().BeTrue();

            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.SecretValues);
            var secretValuesParameters = (TemplateObjectParameterProperties)parametersTemplate.Parameters[ParameterNames.SecretValues];
            var secretValueParameterValues = (Dictionary<string, Dictionary<string, string>>)secretValuesParameters.Value;

            secretValueParameterValues.Count.Should().Be(2);
            secretValueParameterValues.Should().ContainKey(ParameterNames.OpenIdConnectProvidersSecretValues);
            secretValueParameterValues[ParameterNames.OpenIdConnectProvidersSecretValues].Should().ContainKey("originalNameOpenIdConnectProvider");
            secretValueParameterValues[ParameterNames.OpenIdConnectProvidersSecretValues]["originalNameOpenIdConnectProvider"].Should().Be(string.Empty);

            secretValueParameterValues.Should().ContainKey(ParameterNames.IdentityProvidersSecretValues);
            secretValueParameterValues[ParameterNames.IdentityProvidersSecretValues].Should().ContainKey("originalNameIdentityProvider");
            secretValueParameterValues[ParameterNames.IdentityProvidersSecretValues]["originalNameIdentityProvider"].Should().Be(string.Empty);
        }
    }
}
