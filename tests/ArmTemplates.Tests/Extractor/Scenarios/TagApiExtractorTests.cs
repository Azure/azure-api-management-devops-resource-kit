// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients;
using Moq;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Tag Api Extraction")]
    public class TagApiExtractorTests : ExtractorMockerWithOutputTestsBase
    {
        public TagApiExtractorTests() : base("tag-api-tests")
        {
        }

        [Fact]
        public async Task GenerateTagApiTemplates_ProperlyLaysTheInformation()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateTagApiTemplates_ProperlyLaysTheInformation));

            var extractorConfig = this.GetMockedExtractorConsoleAppConfiguration(
                splitApis: false,
                apiVersionSetName: string.Empty,
                multipleApiNames: string.Empty,
                includeAllRevisions: false);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var mockedTagClient = MockTagClient.GetMockedApiClientWithDefaultValues();
            var mockedApiClient = MockApisClient.GetMockedApiClientWithDefaultValues();
            var tagApiExtractor = new TagApiExtractor(
                this.GetTestLogger<TagApiExtractor>(), 
                new TemplateBuilder(),
                mockedApiClient,
                mockedTagClient);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                tagApiExtractor: tagApiExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var tagApiTemplate = await extractorExecutor.GenerateTagApiTemplateAsync(
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.TagApi)).Should().BeTrue();

            tagApiTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            tagApiTemplate.TypedResources.Tags.Count().Should().Be(4);
            tagApiTemplate.Resources.Count().Should().Be(4);

            var resources = tagApiTemplate.TypedResources;

            resources.Tags.Any(x => x.Name.Contains(MockTagClient.TagName1)).Should().BeTrue();
            resources.Tags.Any(x => x.Name.Contains(MockTagClient.TagName2)).Should().BeTrue();
            resources.Tags.All(x => x.ApiVersion == GlobalConstants.ApiVersion).Should().BeTrue();
            resources.Tags.All(x => !x.DependsOn.IsNullOrEmpty() && !string.IsNullOrEmpty(x.DependsOn.First())).Should().BeTrue();
        }
    }
}
