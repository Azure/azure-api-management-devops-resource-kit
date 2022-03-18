using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class PropertyTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreatePropertyFromCreatorConfig()
        {
            // arrange
            var propertyTemplateCreator = new PropertyTemplateCreator(new TemplateBuilder());
            CreatorConfig creatorConfig = new CreatorConfig() { namedValues = new List<PropertyConfig>() };
            PropertyConfig property = new PropertyConfig()
            {
                displayName = "displayName",
                value = "value"
            };
            creatorConfig.namedValues.Add(property);

            // act
            var propertyTemplate = propertyTemplateCreator.CreatePropertyTemplate(creatorConfig);
            var propertyTemplateResource = (PropertyTemplateResource)propertyTemplate.Resources[0];

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{property.displayName}')]", propertyTemplateResource.Name);
            Assert.Equal(property.displayName, propertyTemplateResource.Properties.displayName);
            Assert.Equal(property.value, propertyTemplateResource.Properties.value);
        }
    }
}
