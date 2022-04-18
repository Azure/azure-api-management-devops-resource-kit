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
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor
{
    [Trait("Category", "Output Files Layout")]
    public class OutputFilesLayoutTests : ExtractorMockerWithOutputTestsBase
    {
        public OutputFilesLayoutTests() : base("output-files-layout")
        {
        }

        [Fact]
        public async Task OutputFiles_HaveProperFieldsOrder()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(OutputFiles_HaveProperFieldsOrder));

            // it doesn't matter what template to check - calling policy extractor as an example
            var extractorConfig = this.GetMockedExtractorConsoleAppConfiguration(
                splitApis: false,
                apiVersionSetName: string.Empty,
                multipleApiNames: string.Empty,
                includeAllRevisions: false);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var mockedPolicyApiClient = MockPolicyClient.GetMockedApiClientWithDefaultValues();
            var policyExtractor = new PolicyExtractor(this.GetTestLogger<PolicyExtractor>(), mockedPolicyApiClient, new TemplateBuilder());

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                policyExtractor: policyExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var policyTemplate = await extractorExecutor.GeneratePolicyTemplateAsync(currentTestDirectory);

            // assert
            var outputGlobalServicePolicyFilePath = Path.Combine(currentTestDirectory, extractorParameters.FileNames.GlobalServicePolicy);
            File.Exists(outputGlobalServicePolicyFilePath).Should().BeTrue();

            var policyTemplateText = await File.ReadAllTextAsync(outputGlobalServicePolicyFilePath);
            var policyTemplateJson = JToken.Parse(policyTemplateText);
            
            var resourcesSection = policyTemplateJson["resources"].First;
            resourcesSection.Should().NotBeNull();

            var resourcesSectionText = resourcesSection.ToString();

            var apiVersionFieldPosition = resourcesSectionText.IndexOf("\"apiVersion\"");
            var typeFieldPosition = resourcesSectionText.IndexOf("\"type\"");
            var nameFieldPosition = resourcesSectionText.IndexOf("\"name\"");
            var propertiesFieldPosition = resourcesSectionText.IndexOf("\"properties\"");

            apiVersionFieldPosition.Should().BeLessThan(nameFieldPosition);
            typeFieldPosition.Should().BeLessThan(nameFieldPosition);
            nameFieldPosition.Should().BeLessThan(propertiesFieldPosition);
        }
    }
}
