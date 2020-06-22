using Xunit;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
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
