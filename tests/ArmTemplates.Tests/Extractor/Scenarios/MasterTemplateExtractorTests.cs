// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Xunit;

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

            var extractorConfig = this.GetMockedExtractorConsoleAppConfiguration(
                splitApis: false,
                apiVersionSetName: string.Empty,
                multipleApiNames: string.Empty,
                includeAllRevisions: false);
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

            // act
            var masterTemplate = await extractorExecutor.GenerateMasterTemplateAsync(
                currentTestDirectory,
                tagTemplateResources: tagTemplateResources);

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

            masterTemplate.TypedResources.DeploymentResources.Should().HaveCount(1);
            masterTemplate.Resources.Should().HaveCount(1);

            var tagDeployment = masterTemplate.TypedResources.DeploymentResources.First();

            tagDeployment.Type.Should().Be(ResourceTypeConstants.ArmDeployments);
            tagDeployment.Properties.Should().NotBeNull();
        }
    }
}
