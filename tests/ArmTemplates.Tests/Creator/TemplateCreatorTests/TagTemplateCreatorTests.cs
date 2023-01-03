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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    [Trait("Category", "Tag Template Creation")]
    public class TagTemplateCreatorTests
    {
        CreatorParameters GenerateCreatorParameters(List<string> tagNames = null, string apiTagNames = null)
        {
            var creatorConfig = new CreatorParameters();

            if (!tagNames.IsNullOrEmpty())
            {
                creatorConfig.Tags = new List<TagProperties>();
                foreach (var tagName in tagNames)
                {
                    var tag = new TagProperties
                    {
                        DisplayName = tagName
                    };
                    creatorConfig.Tags.Add(tag);
                }
            }

            if (!apiTagNames.IsNullOrEmpty())
            {
                creatorConfig.Apis = new List<ApiConfig>();

                var apiConfig = new ApiConfig
                {
                    Tags = apiTagNames
                };

                creatorConfig.Apis.Add(apiConfig);
            }

            return creatorConfig;
        }

        [Theory]
        [InlineData("tag 1,tag2,api tag 3,api tag2")]
        [InlineData("tag 1, tag2, api tag 3, api tag2")]
        public void CreateTagTemplate_ShouldCreateTemplateFromCreatorConfig_GivenApiTagsAndConfigTags(string apiTagNames)
        {
            var tagTemplateCreator = new TagTemplateCreator(new TemplateBuilder());
            
            var tagNames = new List<string>()
            {
                "tag 1", "tag2", "tag 3", "tag2"
            };
            var creatorConfig = this.GenerateCreatorParameters(tagNames, apiTagNames);

            var expectedTagsDictionary = new Dictionary<string, string>()
            {
                { "tag-1", "tag 1" },
                { "tag2", "tag2" },
                { "tag-3" , "tag 3" },
                { "api-tag-3", "api tag 3" },
                { "api-tag2", "api tag2" }
            };

            //act
            var tagTemplate = tagTemplateCreator.CreateTagTemplate(creatorConfig);

            //assert
            tagTemplate.Parameters.Count().Should().Be(1);
            tagTemplate.Parameters.ContainsKey(ParameterNames.ApimServiceName).Should().BeTrue();
            tagTemplate.Resources.Count().Should().Be(5);

            var resourcesDictionary = new Dictionary<string, TagTemplateResource>();
            foreach (TagTemplateResource tag in tagTemplate.Resources)
            {
                resourcesDictionary[tag.Name] = tag;
            }

            foreach (var (tagName, tagDisplayName) in expectedTagsDictionary) 
            {
                var resourceName = NamingHelper.GenerateParametrizedResourceName(ParameterNames.ApimServiceName, tagName);
                resourcesDictionary.Should().ContainKey(resourceName);
                var tagResource = resourcesDictionary[resourceName];
                tagResource.Properties.DisplayName.Equals(tagDisplayName).Should().BeTrue();
            }
        }

        [Theory]
        [InlineData("tag 1,tag2,api tag 3,api tag2")]
        [InlineData("tag 1, tag2, api tag 3, api tag2")]
        public void CreateTagTemplate_ShouldCreateTemplateFromCreatorConfig_GivenOnlyApiTags(string apiTagNames)
        {
            var tagTemplateCreator = new TagTemplateCreator(new TemplateBuilder());
            var creatorConfig = this.GenerateCreatorParameters(apiTagNames: apiTagNames);

            var expectedTagsDictionary = new Dictionary<string, string>()
            {
                { "tag-1", "tag 1" },
                { "tag2", "tag2" },
                { "api-tag-3", "api tag 3" },
                { "api-tag2", "api tag2" }
            };

            //act
            var tagTemplate = tagTemplateCreator.CreateTagTemplate(creatorConfig);

            //assert
            tagTemplate.Resources.Count().Should().Be(4);

            var resourcesDictionary = new Dictionary<string, TagTemplateResource>();
            foreach (TagTemplateResource tag in tagTemplate.Resources)
            {
                resourcesDictionary[tag.Name] = tag;
            }

            foreach (var (tagName, tagDisplayName) in expectedTagsDictionary)
            {
                var resourceName = NamingHelper.GenerateParametrizedResourceName(ParameterNames.ApimServiceName, tagName);
                resourcesDictionary.Should().ContainKey(resourceName);
                var tagResource = resourcesDictionary[resourceName];
                tagResource.Properties.DisplayName.Equals(tagDisplayName).Should().BeTrue();
            }
        }

        [Fact]
        public void CreateTagTemplate_ShouldThrowDuplicateTagResourceNameException_GivenDuplicateSanitizedDisplayName()
        {
            var tagTemplateCreator = new TagTemplateCreator(new TemplateBuilder());

            var tagNames= new List<string>()
            {
                "tag 1?", "?tag 1?"
            };

            var creatorConfig = this.GenerateCreatorParameters(tagNames: tagNames);

            //act & assert
            Action act = () => tagTemplateCreator.CreateTagTemplate(creatorConfig);
            act.Should().Throw<DuplicateTagResourceNameException>().WithMessage(string.Format(ErrorMessages.DuplicateTagResourceNameErrorMessage, "tag 1?", "?tag 1?", "tag-1"));
        }

        [Fact]
        public void CreateTagTemplate_ShouldThrowEmptyResourceNameAfterSanitizingErrorMessage_GivenOneEmptyTag()
        {
            var tagTemplateCreator = new TagTemplateCreator(new TemplateBuilder());

            var apiTagNames = "tag 1,tag2,api tag 3,";
            var creatorConfig = this.GenerateCreatorParameters(apiTagNames: apiTagNames);


            //act & assert
            Action act = () => tagTemplateCreator.CreateTagTemplate(creatorConfig);
            act.Should().Throw<EmptyResourceNameException>().WithMessage(string.Format(ErrorMessages.EmptyResourceNameAfterSanitizingErrorMessage, string.Empty)); ;
        }
    }
}