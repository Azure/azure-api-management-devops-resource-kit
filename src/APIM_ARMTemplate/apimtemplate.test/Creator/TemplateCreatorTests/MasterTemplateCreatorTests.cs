using System.Collections.Generic;
using Xunit;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
{
    public class MasterTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateCorrectNumberOfParameterValuesWhenLinked()
        {
            // arrange
            MasterTemplateCreator masterTemplateCreator = MasterTemplateCreatorFactory.GenerateMasterTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig()
            {
                apimServiceName = "apimServiceName",
                linked = true,
                linkedTemplatesBaseUrl = "linkedTemplatesBaseUrl"
            };
            // linked templates result in 2 values
            int count = 2;

            // act
            Template masterTemplate = masterTemplateCreator.CreateMasterTemplateParameterValues(creatorConfig);

            // assert
            Assert.Equal(count, masterTemplate.parameters.Count);
        }

        [Fact]
        public void ShouldCreateCorrectNumberOfParametersWhenUnlinked()
        {
            // arrange
            MasterTemplateCreator masterTemplateCreator = MasterTemplateCreatorFactory.GenerateMasterTemplateCreator();
            bool linked = false;
            // unlinked templates result in 1 value
            int count = 1;

            // act
            Dictionary<string, TemplateParameterProperties> masterTemplateParameters = masterTemplateCreator.CreateMasterTemplateParameters(linked);

            // assert
            Assert.Equal(count, masterTemplateParameters.Keys.Count);
        }

        [Fact]
        public void ShouldCreateLinkedMasterTemplateResourceFromValues()
        {
            // arrange
            MasterTemplateCreator masterTemplateCreator = MasterTemplateCreatorFactory.GenerateMasterTemplateCreator();
            string name = "name";
            string uriLink = "uriLink";
            string[] dependsOn = new string[] { "dependsOn" };

            // act
            MasterTemplateResource masterTemplateResource = masterTemplateCreator.CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);

            // assert
            Assert.Equal(name, masterTemplateResource.name);
            Assert.Equal(uriLink, masterTemplateResource.properties.templateLink.uri);
            Assert.Equal(dependsOn, masterTemplateResource.dependsOn);
        }
    }
}
