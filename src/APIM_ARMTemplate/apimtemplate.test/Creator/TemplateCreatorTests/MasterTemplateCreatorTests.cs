using System.Collections.Generic;
using Xunit;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
{
    public class MasterTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateCorrectNumberOfDeploymentResources()
        {
            // arrange
            CreatorConfig creatorConfig = new CreatorConfig() { apimServiceName = "apimService", linked = true };
            MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator();
            Template apiVersionSetsTemplate = new Template();
            Template globalServicePolicyTemplate = new Template();
            Template productsTemplate = new Template();
            Template propertyTemplate = new Template();
            Template tagTemplate = new Template();
            Template loggersTemplate = new Template();
            List<LinkedMasterTemplateAPIInformation> apiInfoList = new List<LinkedMasterTemplateAPIInformation>() { new LinkedMasterTemplateAPIInformation() { name = "api", isSplit = true } };
            FileNameGenerator fileNameGenerator = new FileNameGenerator();
            FileNames creatorFileNames = fileNameGenerator.GenerateFileNames(creatorConfig.apimServiceName);

            // should create 7 resources (globalServicePolicy, apiVersionSet, product, property, tag, logger, both api templates)
            int count = 8;

            // act
            Template masterTemplate = masterTemplateCreator.CreateLinkedMasterTemplate(creatorConfig, globalServicePolicyTemplate, apiVersionSetsTemplate, productsTemplate, propertyTemplate, loggersTemplate, null, null, tagTemplate, apiInfoList, creatorFileNames, creatorConfig.apimServiceName, fileNameGenerator);

            // assert
            Assert.Equal(count, masterTemplate.resources.Length);
        }

        [Fact]
        public void ShouldCreateCorrectNumberOfParameterValuesWhenLinked()
        {
            // arrange
            MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator();
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
            CreatorConfig creatorConfig = new CreatorConfig() { apimServiceName = "apimService", linked = false };
            MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator();
            // unlinked templates result in 1 value
            int count = 1;

            // act
            Dictionary<string, TemplateParameterProperties> masterTemplateParameters = masterTemplateCreator.CreateMasterTemplateParameters(creatorConfig);

            // assert
            Assert.Equal(count, masterTemplateParameters.Keys.Count);
        }

        [Fact]
        public void ShouldCreateLinkedMasterTemplateResourceFromValues()
        {
            // arrange
            MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator();
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
        public void ShouldCreateCorrectLinkedUri()
        {
            // arrange
            MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { apimServiceName = "apimService", linked = true, linkedTemplatesBaseUrl = "http://someurl.com", linkedTemplatesUrlQueryString = "?param=1" };
            string apiVersionSetFileName = "/versionSet1-apiVersionSets.template.json";

            // act
            string linkedResourceUri = masterTemplateCreator.GenerateLinkedTemplateUri(creatorConfig, apiVersionSetFileName);

            // assert
            Assert.Equal($"[concat(parameters('LinkedTemplatesBaseUrl'), '{apiVersionSetFileName}', parameters('LinkedTemplatesUrlQueryString'))]", linkedResourceUri);
        }
    }
}
