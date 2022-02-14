using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class PropertyTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreatePropertyFromCreatorConfig()
        {
            // arrange
            var propertyTemplateCreator = new PropertyTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { namedValues = new List<PropertyConfig>() };
            PropertyConfig property = new PropertyConfig()
            {
                displayName = "displayName",
                value = "value"
            };
            creatorConfig.namedValues.Add(property);

            // act
            var propertyTemplate = propertyTemplateCreator.CreatePropertyTemplate(creatorConfig);
            var propertyTemplateResource = (PropertyTemplateResource)propertyTemplate.resources[0];

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{property.displayName}')]", propertyTemplateResource.name);
            Assert.Equal(property.displayName, propertyTemplateResource.properties.displayName);
            Assert.Equal(property.value, propertyTemplateResource.properties.value);
        }
    }
}
