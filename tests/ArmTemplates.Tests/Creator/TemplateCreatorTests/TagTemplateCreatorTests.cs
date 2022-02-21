using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class TagTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateTagFromCreatorConfig()
        {
            TagTemplateCreator tagTemplateCreator = new TagTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { tags = new List<TagTemplateProperties>() };
            TagTemplateProperties tag = new TagTemplateProperties()
            {
                displayName = "displayName"
            };
            creatorConfig.tags.Add(tag);

            //act
            var tagTemplate = tagTemplateCreator.CreateTagTemplate(creatorConfig);
            var tagTemplateResource = (TagTemplateResource)tagTemplate.Resources[0];

            //assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{tag.displayName}')]", tagTemplateResource.Name);
        }
    }
}