using Xunit;
using System.Collections.Generic;
using apimtemplate.Common.Templates.Abstractions;
using apimtemplate.Creator.TemplateCreators;
using apimtemplate.Common.TemplateModels;
using apimtemplate.Creator.Models;

namespace apimtemplate.test.Creator.TemplateCreatorTests
{
    public class PropertyTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreatePropertyFromCreatorConfig()
        {
            // arrange
            PropertyTemplateCreator propertyTemplateCreator = new PropertyTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { namedValues = new List<PropertyConfig>() };
            PropertyConfig property = new PropertyConfig()
            {
                displayName = "displayName",
                value = "value"
            };
            creatorConfig.namedValues.Add(property);

            // act
            Template propertyTemplate = propertyTemplateCreator.CreatePropertyTemplate(creatorConfig);
            PropertyTemplateResource propertyTemplateResource = (PropertyTemplateResource)propertyTemplate.resources[0];

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{property.displayName}')]", propertyTemplateResource.name);
            Assert.Equal(property.displayName, propertyTemplateResource.properties.displayName);
            Assert.Equal(property.value, propertyTemplateResource.properties.value);
        }
    }
}
