// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class TagTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateTagFromCreatorConfig()
        {
            TagTemplateCreator tagTemplateCreator = new TagTemplateCreator(new TemplateBuilder());
            CreatorConfig creatorConfig = new CreatorConfig() { tags = new List<TagProperties>() };
            TagProperties tag = new TagProperties()
            {
                DisplayName = "displayName"
            };
            creatorConfig.tags.Add(tag);

            //act
            var tagTemplate = tagTemplateCreator.CreateTagTemplate(creatorConfig);
            var tagTemplateResource = (TagTemplateResource)tagTemplate.Resources[0];

            //assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{tag.DisplayName}')]", tagTemplateResource.Name);
        }
    }
}