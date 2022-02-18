using Xunit;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
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
            Template tagTemplate = tagTemplateCreator.CreateTagTemplate(creatorConfig);
            TagTemplateResource tagTemplateResource = (TagTemplateResource)tagTemplate.resources[0];

            //assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{tag.displayName}')]", tagTemplateResource.name);
        }
    }
}