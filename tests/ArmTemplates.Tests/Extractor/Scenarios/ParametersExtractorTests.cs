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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;

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
            var parametersExtractor = new ParametersExtractor(this.GetTestLogger<ParametersExtractor>(), new TemplateBuilder(), apisClient, identityProviderClient, openIdConnectProviderClient);

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

        [Fact]
        public async Task GenerateResourceParametersFiles_ProperlyGeneratesRenmaesAndCreatesNewDirectoryWithParameterFile_GivenItsCalledTwice()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateResourceParametersFiles_ProperlyGeneratesRenmaesAndCreatesNewDirectoryWithParameterFile_GivenItsCalledTwice));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration();
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var extractorExecutor = this.GetExtractorInstance(extractorParameters, null);

            var resourceTemplate = new Template<OpenIdConnectProviderResources>()
            {
                Parameters = new Dictionary<string, TemplateParameterProperties>() {
                    { "parameter1", new () },
                    { "parameter2", new () }
                },
                TypedResources = new OpenIdConnectProviderResources()
                {
                    OpenIdConnectProviders = new List<OpenIdConnectProviderResource>()
                    {
                        new(),
                        new(),
                    }
                }
            };

            var mainParameterTemplate = new Template()
            {
                Parameters = new Dictionary<string, TemplateParameterProperties>() {
                    { "parameter1", new () },
                    { "parameter2", new () },
                    { "parameter3", new () },
                    { "parameter4", new () },
                    { "parameter5", new () },
                }
            };

            // act
            await extractorExecutor.GenerateResourceParametersFiles(currentTestDirectory, mainParameterTemplate, openIdConnectProviderTemplate: resourceTemplate);
            Directory.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.ParametersDirectory)).Should().BeTrue();
            var creationDateTime = Directory.GetCreationTime(Path.Combine(currentTestDirectory, extractorParameters.FileNames.ParametersDirectory)).ToString("yyyyMMddHHmmss");

            await extractorExecutor.GenerateResourceParametersFiles(currentTestDirectory, mainParameterTemplate, openIdConnectProviderTemplate: resourceTemplate);
            Directory.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.ParametersDirectory)).Should().BeTrue();
            Directory.Exists(Path.Combine(currentTestDirectory, $"{extractorParameters.FileNames.ParametersDirectory}{creationDateTime}")).Should().BeTrue();
        }

        [Fact]
        public async Task GenerateResourceParametersFile_DoesNotRaiseException_GivenNullFilenames()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateResourceParametersFile_DoesNotRaiseException_GivenNullFilenames));
            var parameterFileName = "filename.json";

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration();
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var extractorExecutor = this.GetExtractorInstance(extractorParameters, null);

            var resourceTemplate = new Template<OpenIdConnectProviderResources>()
            {
                Parameters = new Dictionary<string, TemplateParameterProperties>() {
                    { "parameter1", new () },
                    { "parameter2", new () }
                }
            };

            // act
            await extractorExecutor.GenerateResourceParametersFile(parameterFileName, null, resourceTemplate, null);

            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.ParametersDirectory, parameterFileName)).Should().BeFalse();
        }

        [Fact]
        public void CreateResourceTemplateParameterTemplate_ProperlyGeneratesTemplate_WithFilteredParameters()
        {
            // arrange

            var parameterExtractor = new ParametersExtractor(this.GetTestLogger<ParametersExtractor>(), new TemplateBuilder(), default, default, default);

            var resourceTemplate = new Template<OpenIdConnectProviderResources>()
            {
                Parameters = new Dictionary<string, TemplateParameterProperties>() {
                    { "parameter1", new () },
                    { "parameter2", new () }
                }
            };

            var mainParameterTemplate = new Template()
            {
                Parameters = new Dictionary<string, TemplateParameterProperties>() {
                    { "parameter1", new () },
                    { "parameter2", new () },
                    { "parameter3", new () },
                    { "parameter4", new () },
                    { "parameter5", new () },
                }
            };

            // act
            var resourceTemplateParameterTemplate = parameterExtractor.CreateResourceTemplateParameterTemplate(resourceTemplate, mainParameterTemplate);

            // assert
            resourceTemplateParameterTemplate.Parameters.Should().HaveCount(2);
            resourceTemplateParameterTemplate.Parameters["parameter1"].Should().NotBeNull();
            resourceTemplateParameterTemplate.Parameters["parameter2"].Should().NotBeNull();
        }

        [Fact]
        public async Task CreateResourceTemplateParameterTemplate_ProperlyGeneratesTemplate_WithBackendProxySettings()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(CreateResourceTemplateParameterTemplate_ProperlyGeneratesTemplate_WithBackendProxySettings));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(paramBackend: "true");
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var extractorExecutor = this.GetExtractorInstance(extractorParameters, null);

            var backends = new BackendTemplateResources()
            {
                BackendNameParametersCache = new Dictionary<string, BackendApiParameters>()
                {
                    { "key1", new BackendApiParameters() { Protocol = "protocol", ResourceId = "resourceId", Url = "url" } }
                },
                BackendProxyParametersCache = new Dictionary<string, BackendProxyParameters>()
                {
                    { "key1", new BackendProxyParameters() { Username = "username", Url = "url" } }
                },
                Backends = new List<BackendTemplateResource>()
                {
                    {
                        new BackendTemplateResource
                        {
                            Name = "test"
                        }
                    }
                }
            };

            // act
            var parametersTemplate = await extractorExecutor.GenerateParametersTemplateAsync(null, null, backends, null, new IdentityProviderResources(), new OpenIdConnectProviderResources(), currentTestDirectory);

            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Parameters)).Should().BeTrue();

            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.BackendProxy);
            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.BackendSettings);

            var backendProxyParameters = (TemplateObjectParameterProperties)parametersTemplate.Parameters[ParameterNames.BackendProxy];
            var backendProxyParameterValues = (Dictionary<string, BackendProxyParameters>)backendProxyParameters.Value;

            backendProxyParameterValues.Should().NotBeNull();
            backendProxyParameterValues.ContainsKey("key1").Should().BeTrue();
            backendProxyParameterValues["key1"].Username.Should().Be("username");
            backendProxyParameterValues["key1"].Url.Should().Be("url");


            var backendSettingsParameters = (TemplateObjectParameterProperties)parametersTemplate.Parameters[ParameterNames.BackendSettings];
            var backendSettingsParameterValues = (Dictionary<string, BackendApiParameters>)backendSettingsParameters.Value;

            backendSettingsParameterValues.Should().NotBeNull();
            backendSettingsParameterValues.ContainsKey("key1").Should().BeTrue();
            backendSettingsParameterValues["key1"].Protocol.Should().Be("protocol");
            backendSettingsParameterValues["key1"].ResourceId.Should().Be("resourceId");
            backendSettingsParameterValues["key1"].Url.Should().Be("url");
        }

        [Fact]
        public async Task GenerateParametersTemplates_DoesNotAddNamedValuesParameters_GivenParametrizeConfigNotPassed()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateParametersTemplates_DoesNotAddNamedValuesParameters_GivenParametrizeConfigNotPassed));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration();
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var extractorExecutor = this.GetExtractorInstance(extractorParameters, null);

            // act
            var parametersTemplate = await extractorExecutor.GenerateParametersTemplateAsync(null, null, null, null, new IdentityProviderResources(), new OpenIdConnectProviderResources(), currentTestDirectory);

            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Parameters)).Should().BeTrue();

            parametersTemplate.Parameters.Count.Should().Be(1);
            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            parametersTemplate.Parameters.Should().NotContainKey(ParameterNames.NamedValues);
            parametersTemplate.Parameters.Should().NotContainKey(ParameterNames.NamedValueKeyVaultSecrets);
        }

        [Fact]
        public async Task GenerateParametersTemplates_ContainsNamedValuesParameters_GivenParametrizeNamedValueConfigIsTrue()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateParametersTemplates_ContainsNamedValuesParameters_GivenParametrizeNamedValueConfigIsTrue));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(paramNamedValue: "true");
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var extractorExecutor = this.GetExtractorInstance(extractorParameters, null);

            var namedValuesResources = new NamedValuesResources()
            {
                NamedValues = new List<NamedValueTemplateResource>()
                {
                    new NamedValueTemplateResource()
                    {
                        Properties = new NamedValueProperties() 
                        {
                            Secret = false,
                            Value = "named-value-1",
                            OriginalValue= "named-value-1"
                        },
                        Name = "named-value-name-1",
                        OriginalName = "named-value-name-1"
                    },
                    new NamedValueTemplateResource()
                    {
                        Properties = new NamedValueProperties()
                        {
                            Secret = true,
                            Value = null,
                            OriginalValue= null
                        },
                        Name = "named-value-name-2",
                        OriginalName = "named-value-name-2"
                    }
                }
            };

            // act
            var parametersTemplate = await extractorExecutor.GenerateParametersTemplateAsync(null, null, null, namedValuesResources, new IdentityProviderResources(), new OpenIdConnectProviderResources(), currentTestDirectory);

            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Parameters)).Should().BeTrue();

            parametersTemplate.Parameters.Count.Should().Be(2);
            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.NamedValues);

            var namedValuesSettingsParameters = (TemplateObjectParameterProperties)parametersTemplate.Parameters[ParameterNames.NamedValues];
            var namedValuesSettingsParametersValues = (Dictionary<string, string>)namedValuesSettingsParameters.Value;

            namedValuesSettingsParametersValues.Count.Should().Be(2);
            namedValuesSettingsParametersValues[NamingHelper.GenerateValidParameterName("named-value-name-1", ParameterPrefix.Property)].Should().Be("named-value-1");
            namedValuesSettingsParametersValues[NamingHelper.GenerateValidParameterName("named-value-name-2", ParameterPrefix.Property)].Should().Be(null);
        }

        [Fact]
        public async Task GenerateParametersTemplates_ContainsNamedValuesParameters_GivenParametrizeNamedValueAndExtractSecretsConfigAreTrue()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateParametersTemplates_ContainsNamedValuesParameters_GivenParametrizeNamedValueAndExtractSecretsConfigAreTrue));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(paramNamedValue: "true", extractSecrets: "true");
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var extractorExecutor = this.GetExtractorInstance(extractorParameters, null);

            var namedValuesResources = new NamedValuesResources()
            {
                NamedValues = new List<NamedValueTemplateResource>()
                {
                    new NamedValueTemplateResource()
                    {
                        Properties = new NamedValueProperties()
                        {
                            Secret = false,
                            Value = "named-value-1",
                            OriginalValue= "named-value-1"
                        },
                        Name = "named-value-name-1",
                        OriginalName = "named-value-name-1"
                    },
                    new NamedValueTemplateResource()
                    {
                        Properties = new NamedValueProperties()
                        {
                            Secret = true,
                            Value = "extracted-secret",
                            OriginalValue = "extracted-secret"
                        },
                        Name = "named-value-name-2",
                        OriginalName = "named-value-name-2"
                    }
                }
            };

            // act
            var parametersTemplate = await extractorExecutor.GenerateParametersTemplateAsync(null, null, null, namedValuesResources, new IdentityProviderResources(), new OpenIdConnectProviderResources(), currentTestDirectory);

            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Parameters)).Should().BeTrue();

            parametersTemplate.Parameters.Count.Should().Be(2);
            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.NamedValues);

            var namedValuesSettingsParameters = (TemplateObjectParameterProperties)parametersTemplate.Parameters[ParameterNames.NamedValues];
            var namedValuesSettingsParametersValues = (Dictionary<string, string>)namedValuesSettingsParameters.Value;

            namedValuesSettingsParametersValues.Count.Should().Be(2);
            namedValuesSettingsParametersValues[NamingHelper.GenerateValidParameterName("named-value-name-1", ParameterPrefix.Property)].Should().Be("named-value-1");
            namedValuesSettingsParametersValues[NamingHelper.GenerateValidParameterName("named-value-name-2", ParameterPrefix.Property)].Should().Be("extracted-secret");
        }

        [Fact]
        public async Task GenerateParametersTemplates_ContainsNamedValuesKeyVaultSecretParameters_GivenParametrizeNamedValueKeyVaultSecretsConfigIsTrue()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateParametersTemplates_ContainsNamedValuesKeyVaultSecretParameters_GivenParametrizeNamedValueKeyVaultSecretsConfigIsTrue));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(paramNamedValuesKeyVaultSecrets: "true");
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var extractorExecutor = this.GetExtractorInstance(extractorParameters, null);

            var namedValuesResources = new NamedValuesResources()
            {
                NamedValues = new List<NamedValueTemplateResource>()
                {
                    new NamedValueTemplateResource()
                    {
                        Properties = new NamedValueProperties()
                        {
                            Secret = false,
                            Value = "named-value-1",
                            OriginalValue= "named-value-1"
                        },
                        Name = "named-value-name-1",
                        OriginalName = "named-value-name-1"
                    },
                    new NamedValueTemplateResource()
                    {
                        Properties = new NamedValueProperties()
                        {
                            Secret = true,
                            Value = null,
                            OriginalValue = null
                        },
                        Name = "named-value-name-2",
                        OriginalName = "named-value-name-2"
                    },
                    new NamedValueTemplateResource()
                    {
                        Properties = new NamedValueProperties()
                        {
                            Secret = true,
                            Value = null,
                            OriginalValue = null,
                            OriginalKeyVaultSecretIdentifierValue = "secret-oidentifier-value-original",
                            KeyVault = new NamedValueResourceKeyVaultProperties()
                            {
                                SecretIdentifier = "secret-oidentifier-value"
                            }
                        },
                        Name = "key-vault-named-value-name-1",
                        OriginalName = "key-vault-named-value-name-1"
                    }
                }
            };

            // act
            var parametersTemplate = await extractorExecutor.GenerateParametersTemplateAsync(null, null, null, namedValuesResources, new IdentityProviderResources(), new OpenIdConnectProviderResources(), currentTestDirectory);

            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Parameters)).Should().BeTrue();

            parametersTemplate.Parameters.Count.Should().Be(2);
            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            parametersTemplate.Parameters.Should().ContainKey(ParameterNames.NamedValueKeyVaultSecrets);

            var namedValuesKeyVaultsSettingsParameters = (TemplateObjectParameterProperties)parametersTemplate.Parameters[ParameterNames.NamedValueKeyVaultSecrets];
            var namedValuesKeyVaultSettingsParametersValues = (Dictionary<string, string>)namedValuesKeyVaultsSettingsParameters.Value;

            namedValuesKeyVaultSettingsParametersValues.Count.Should().Be(1);
            namedValuesKeyVaultSettingsParametersValues[NamingHelper.GenerateValidParameterName("key-vault-named-value-name-1", ParameterPrefix.Property)].Should().Be("secret-oidentifier-value-original");
        }
    }
}
