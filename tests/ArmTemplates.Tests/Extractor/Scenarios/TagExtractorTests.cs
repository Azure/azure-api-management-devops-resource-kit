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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Tag Extraction")]
    public class TagExtractorTests : ExtractorMockerWithOutputTestsBase
    {
        public TagExtractorTests() : base("tag-tests")
        {
        }

        [Fact]
        public async Task GenerateTagTemplates_GetAll_ProperlyLaysTheInformation()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateTagTemplates_GetAll_ProperlyLaysTheInformation));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration();
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var mockedTagClient = MockTagClient.GetMockedApiClientWithDefaultValues();
            var tagExtractor = new TagExtractor(
                this.GetTestLogger<TagExtractor>(),
                mockedTagClient,
                new TemplateBuilder());

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                tagExtractor: tagExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            ApiTemplateResources apiTemplateResources = new ApiTemplateResources();
            ProductTemplateResources productTemplateResources = new ProductTemplateResources();

            var tagTemplate = await extractorExecutor.GenerateTagTemplateAsync(
                string.Empty,
                apiTemplateResources,
                productTemplateResources,
                currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Tags)).Should().BeTrue();

            tagTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            tagTemplate.TypedResources.Tags.Count().Should().Be(4);
            tagTemplate.Resources.Count().Should().Be(4);

            var resources = tagTemplate.TypedResources;

            resources.Tags.Any(x => x.Name.Contains(MockTagClient.TagName1)).Should().BeTrue();
            resources.Tags.Any(x => x.Name.Contains(MockTagClient.TagName2)).Should().BeTrue();
            resources.Tags.Any(x => x.Name.Contains(MockTagClient.OperationTagName1)).Should().BeTrue();
            resources.Tags.Any(x => x.Name.Contains(MockTagClient.OperationTagName2)).Should().BeTrue();
            resources.Tags.All(x => x.DependsOn.IsNullOrEmpty()).Should().BeTrue();
        }

        [Fact]
        public async Task GenerateTagTemplates_GetApiOperationRelated_ProperlyLaysTheInformation()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateTagTemplates_GetApiOperationRelated_ProperlyLaysTheInformation));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration();
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var mockedTagClient = MockTagClient.GetMockedApiClientWithDefaultValues();
            var tagExtractor = new TagExtractor(
                this.GetTestLogger<TagExtractor>(),
                mockedTagClient,
                new TemplateBuilder());

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                tagExtractor: tagExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            ApiTemplateResources apiTemplateResources = new ApiTemplateResources()
            {
                ApiOperationsTags = new List<TagTemplateResource>()
                {
                    new TagTemplateResource()
                    {
                        Name = $"parameters'/{MockTagClient.OperationTagName1}'"
                    },
                    new TagTemplateResource()
                    {
                        Name = $"parameters/{MockTagClient.OperationTagName2}'"
                    }
                }
            };
            ProductTemplateResources productTemplateResources = new ProductTemplateResources();

            var tagTemplate = await extractorExecutor.GenerateTagTemplateAsync(
                "apiName1",
                apiTemplateResources,
                productTemplateResources,
                currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Tags)).Should().BeTrue();

            tagTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            tagTemplate.TypedResources.Tags.Count().Should().Be(2);
            tagTemplate.Resources.Count().Should().Be(2);

            var resources = tagTemplate.TypedResources;

            resources.Tags.Any(x => x.Name.Contains(MockTagClient.OperationTagName1)).Should().BeTrue();
            resources.Tags.Any(x => x.Name.Contains(MockTagClient.OperationTagName2)).Should().BeTrue();
        }
    }
}
