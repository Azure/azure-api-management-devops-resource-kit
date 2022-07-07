// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Groups;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Xunit;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.IdentityProviders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Schemas;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Master Template Extraction")]
    public class MasterTemplateExtractorTests : ExtractorMockerWithOutputTestsBase
    {
        public MasterTemplateExtractorTests() : base("master-template-tests")
        {
        }

        [Fact]
        public async Task GenerateMasterTemplates_ProperlyLaysTheInformation()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateMasterTemplates_ProperlyLaysTheInformation));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                multipleAPIs: string.Empty,
                apiVersionSetName: string.Empty,
                includeAllRevisions: "false",
                splitAPIs: "false",
                policyXmlBaseUrl: string.Empty,
                policyXmlSasToken: string.Empty,
                linkedTemplatesBaseUrl: "linkedBaseUrl",
                linkedTemplatesSasToken: "linkedUrlToken",
                linkedTemplatesUrlQueryString: "queryString",
                apiParameters: new Dictionary<string, ApiParameterProperty> { { "test-service-url-property-api-name", new ApiParameterProperty(null, "test-service-url-property-url") } },
                paramServiceUrl: "true",
                paramNamedValue: "true",
                paramApiLoggerId: "true",
                paramLogResourceId: "true",
                serviceBaseUrl: "test-service-base-url",
                notIncludeNamedValue: "true",
                paramNamedValuesKeyVaultSecrets: "true",
                paramBackend: "true",
                extractGateways: "true",
                paramApiOauth2Scope: "true",
                extractSecrets: "false"
                );

            var extractorParameters = new ExtractorParameters(extractorConfig);

            var masterTemplateExtractor = new MasterTemplateExtractor(
                this.GetTestLogger<MasterTemplateExtractor>(),
                new TemplateBuilder());

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                masterTemplateExtractor: masterTemplateExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            var tagTemplateResources = new TagTemplateResources()
            {
                Tags = new()
                {
                    new TagTemplateResource()
                    {
                        Name = "my-tag",
                        Properties = new() { DisplayName = "my-display-tag" }
                    }
                }
            };

            var groupTemplateResources = new GroupTemplateResources()
            {
                Groups = new()
                {
                    new GroupTemplateResource()
                    {
                        Name = "test-group",
                        Properties = new() {
                            DisplayName = "group-display-name",
                            Description = "group-description",
                            Type = "group-type",
                            ExternalId = "group-external-id",
                            BuiltIn = false
                        }
                    }
                }
            };

            var identityProviderResources = new IdentityProviderResources()
            {
                IdentityProviders = new()
                {
                    new IdentityProviderResource()
                    {
                        OriginalName = "originalName"
                    }
                }
            };

            var schemaResources = new SchemaTemplateResources()
            {
                Schemas = new()
                {
                    new SchemaTemplateResource()
                    {
                        OriginalName = "originalName",
                    }
                }
            };

            // act
            var masterTemplate = await extractorExecutor.GenerateMasterTemplateAsync(
                currentTestDirectory,
                tagTemplateResources: tagTemplateResources,
                groupTemplateResources: groupTemplateResources,
                identityProviderTemplateResources: identityProviderResources,
                schemaTemplateResources: schemaResources);

            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.LinkedMaster)).Should().BeTrue();

            masterTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            masterTemplate.Parameters.Should().ContainKey(ParameterNames.LinkedTemplatesBaseUrl);
            masterTemplate.Parameters.Should().ContainKey(ParameterNames.LinkedTemplatesSasToken);
            masterTemplate.Parameters.Should().ContainKey(ParameterNames.LinkedTemplatesUrlQueryString);
            masterTemplate.Parameters.Should().ContainKey(ParameterNames.ServiceUrl);
            masterTemplate.Parameters.Should().ContainKey(ParameterNames.NamedValues);
            masterTemplate.Parameters.Should().ContainKey(ParameterNames.ApiLoggerId);
            masterTemplate.Parameters.Should().ContainKey(ParameterNames.LoggerResourceId);
            masterTemplate.Parameters.Should().ContainKey(ParameterNames.NamedValueKeyVaultSecrets);
            masterTemplate.Parameters.Should().ContainKey(ParameterNames.BackendSettings);
            masterTemplate.Parameters.Should().ContainKey(ParameterNames.PolicyXMLBaseUrl);
            masterTemplate.Parameters.Should().ContainKey(ParameterNames.PolicyXMLBaseUrl);
            masterTemplate.Parameters.Should().ContainKey(ParameterNames.ApiOauth2ScopeSettings);
            masterTemplate.Parameters.Should().ContainKey(ParameterNames.SecretValues);

            masterTemplate.TypedResources.DeploymentResources.Should().HaveCount(4);
            masterTemplate.Resources.Should().HaveCount(4);

            foreach(var deploymentResource in masterTemplate.TypedResources.DeploymentResources) {
                deploymentResource.Type.Should().Be(ResourceTypeConstants.ArmDeployments);
                deploymentResource.Properties.Should().NotBeNull();
            }
        }
    }
}
