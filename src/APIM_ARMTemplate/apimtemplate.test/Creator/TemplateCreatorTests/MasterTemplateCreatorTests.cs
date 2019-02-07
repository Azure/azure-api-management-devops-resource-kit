using System.Collections.Generic;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
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

        [Fact]
        public void ShouldCreateCorrectNumberOfResourcesInSubsequentUnlinkedMasterTemplate()
        {
            // arrange
            MasterTemplateCreator masterTemplateCreator = MasterTemplateCreatorFactory.GenerateMasterTemplateCreator();
            Template subsequentAPITemplate = new Template();
            List<TemplateResource> templateResources = new List<TemplateResource>();
            int count = 3;
            for (int i = 0; i < count; i++)
            {
                templateResources.Add(new TemplateResource());
            };
            subsequentAPITemplate.resources = templateResources.ToArray();

            // act
            Template subsequentUnlinkedMasterTemplate = masterTemplateCreator.CreateSubsequentUnlinkedMasterTemplate(subsequentAPITemplate);

            // assert
            Assert.Equal(count, subsequentUnlinkedMasterTemplate.resources.Length);
        }

        [Fact]
        public void ShouldCreateCorrectNumberOfResourcesInInitialUnlinkedMasterTemplate()
        {
            // arrange
            MasterTemplateCreator masterTemplateCreator = MasterTemplateCreatorFactory.GenerateMasterTemplateCreator();
            Template initialAPITemplate = new Template();
            List<TemplateResource> initialAPITemplateResources = new List<TemplateResource>();
            int apiTemplateCount = 1;
            for (int i = 0; i < apiTemplateCount; i++)
            {
                initialAPITemplateResources.Add(new TemplateResource());
            };
            initialAPITemplate.resources = initialAPITemplateResources.ToArray();

            Template apiVersionSetTemplate = new Template();
            List<TemplateResource> apiVersionSetResources = new List<TemplateResource>();
            int apiVersionSetTemplateCount = 1;
            for (int i = 0; i < apiVersionSetTemplateCount; i++)
            {
                apiVersionSetResources.Add(new TemplateResource());
            };
            apiVersionSetTemplate.resources = apiVersionSetResources.ToArray();

            int count = apiTemplateCount + apiVersionSetTemplateCount;

            // act
            Template initialLinkedMasterTemplate = masterTemplateCreator.CreateInitialUnlinkedMasterTemplate(apiVersionSetTemplate, initialAPITemplate);

            // assert
            Assert.Equal(count, initialLinkedMasterTemplate.resources.Length);
        }
    }
}
