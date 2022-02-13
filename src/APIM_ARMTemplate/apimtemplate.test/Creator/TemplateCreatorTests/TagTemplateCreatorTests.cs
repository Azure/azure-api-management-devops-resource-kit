using Xunit;
using System.Collections.Generic;
using apimtemplate.Common.Templates.Abstractions;
using apimtemplate.Creator.Models;
using apimtemplate.Common.TemplateModels;
using apimtemplate.Creator.TemplateCreators;

namespace apimtemplate.test.Creator.TemplateCreatorTests
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