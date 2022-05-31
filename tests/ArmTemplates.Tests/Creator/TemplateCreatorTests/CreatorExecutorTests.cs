// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    [Trait("Category", "Creator teamplate execution")]
    public class CreatorExecutorTests : CreatorMockerWithOutputTestsBase
    {
        public CreatorExecutorTests() : base("creator-executor-test")
        {
        }

        [Fact]
        public async Task GenerateTagsTemplate_ShouldCreateTemplateFromCreatorConfig_GivenOnlyApiTagsAndConfigTags()
        {
            var tagTemplateCreator = new TagTemplateCreator(new TemplateBuilder());
            var creatorConfig = new CreatorParameters();

            creatorConfig.Apis = new List<ApiConfig>();
            var apiConfig = new ApiConfig
            {
                Tags = "tag 1"
            };
            creatorConfig.Apis.Add(apiConfig);
            creatorConfig.GenerateFileNames();

            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateTagsTemplate_ShouldCreateTemplateFromCreatorConfig_GivenOnlyApiTagsAndConfigTags));
            creatorConfig.OutputLocation = currentTestDirectory;

            var creatorExecutor = CreatorExecutor.BuildCreatorExecutor(this.GetTestLogger<CreatorExecutor>(), null, tagTemplateCreator: tagTemplateCreator);
            creatorExecutor.SetCreatorParameters(creatorConfig);
            
            //act
            var tagTemplate = await creatorExecutor.GenerateTagsTemplate();

            //assert
            tagTemplate.Should().NotBeNull();
            File.Exists(Path.Combine(currentTestDirectory, creatorConfig.FileNames.Tags)).Should().BeTrue();
        }
    }
}