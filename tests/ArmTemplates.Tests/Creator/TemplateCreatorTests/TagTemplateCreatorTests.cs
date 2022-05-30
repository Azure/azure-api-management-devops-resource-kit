// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters;
using System.Linq;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class TagTemplateCreatorTests
    {
        [Fact]
        public void CreateTagTemplate_ShouldCreateTemplateFromCreatorConfig_GivenApiTagsAndConfigTags()
        {
            var tagTemplateCreator = new TagTemplateCreator(new TemplateBuilder());
            
            var creatorConfig = new CreatorParameters() 
            { 
                Tags = new List<TagProperties>(),
                Apis = new List<ApiConfig>()
            };

            var tagNames = new List<string>()
            {
                "tag 1", "tag2", "tag 3", "tag2"
            };
            var apiTagNames = new List<string>()
            {
                "tag 1", "tag2", "api tag 3", "api tag2"
            };
            var apiTagNamesString = string.Join(", ", apiTagNames.ToArray());

            var expectedTagDictionary = new Dictionary<string, string>() 
            {
                { "tag-1", "tag 1" },
                { "tag2", "tag2" },
                { "tag-3" , "tag 3" },
                { "api-tag-3", "api tag 3" },
                { "api-tag2", "api tag2"}
            };

            foreach (var tagName in tagNames)
            {
                var tag = new TagProperties
                {
                    DisplayName = tagName
                };
                creatorConfig.Tags.Add(tag);
            }

            var apiConfig = new ApiConfig
            {
                Tags = apiTagNamesString
            };
            creatorConfig.Apis.Add(apiConfig);

            //act
            var tagTemplate = tagTemplateCreator.CreateTagTemplate(creatorConfig);

            //assert
            tagTemplate.Parameters.Count().Should().Be(1);
            tagTemplate.Parameters.ContainsKey(ParameterNames.ApimServiceName).Should().BeTrue();

            tagTemplate.Resources.Count().Should().Be(5);
            
            foreach (TagTemplateResource tag in tagTemplate.Resources)
            {
                var resourceTagName = tag.Name.Split("/")[1].Split("'")[0];
                expectedTagDictionary.Should().ContainKey(resourceTagName);
                var generatedTagDisplayName = expectedTagDictionary[resourceTagName];
                tag.Properties.DisplayName.Equals(generatedTagDisplayName).Should().BeTrue();
            }
        }

        [Fact]
        public void CreateTagTemplate_ShouldCreateTemplateFromCreatorConfig_GivenOnlyApiTags()
        {
            var tagTemplateCreator = new TagTemplateCreator(new TemplateBuilder());

            var creatorConfig = new CreatorParameters()
            {
                Apis = new List<ApiConfig>()
            };

            var apiTagNames = new List<string>()
            {
                "tag 1", "tag2", "api tag 3", "api tag2"
            };
            var apiTagNamesString = string.Join(", ", apiTagNames.ToArray());

            var expectedTagsDictionary = new Dictionary<string, string>()
            {
                { "tag-1", "tag 1" },
                { "tag2", "tag2" },
                { "api-tag-3", "api tag 3" },
                { "api-tag2", "api tag2"}
            };

            var outputTags = new HashSet<string>();
            outputTags.UnionWith(apiTagNames);

            var apiConfig = new ApiConfig
            {
                Tags = apiTagNamesString
            };
            creatorConfig.Apis.Add(apiConfig);

            //act
            var tagTemplate = tagTemplateCreator.CreateTagTemplate(creatorConfig);

            //assert
            tagTemplate.Resources.Count().Should().Be(4);

            foreach (TagTemplateResource tag in tagTemplate.Resources)
            {
                var resourceTagName = tag.Name.Split("/")[1].Split("'")[0];
                expectedTagsDictionary.Should().ContainKey(resourceTagName);
                var generatedTagDisplayName = expectedTagsDictionary[resourceTagName];
                tag.Properties.DisplayName.Equals(generatedTagDisplayName).Should().BeTrue();
            }
        }

        [Fact]
        public void CreateTagTemplate_ShouldThrowDuplicateTagResourceNameException_GivenDuplicateSanitizedDisplayName()
        {
            var tagTemplateCreator = new TagTemplateCreator(new TemplateBuilder());

            var creatorConfig = new CreatorParameters()
            {
                Tags = new List<TagProperties>()
            };

            var tagNames= new List<string>()
            {
                "tag 1?", "?tag 1?"
            };

            foreach (var tagName in tagNames)
            {
                var tag = new TagProperties
                {
                    DisplayName = tagName
                };
                creatorConfig.Tags.Add(tag);
            }

            //act & assert
            var exception = Assert.Throws<DuplicateTagResourceNameException>(() => tagTemplateCreator.CreateTagTemplate(creatorConfig));
            exception.Message.Equals(string.Format(ErrorMessages.DuplicateTagResourceNameErrorMessage, "tag 1?", "?tag 1?", "tag-1")).Should().BeTrue();
        }
    }
}