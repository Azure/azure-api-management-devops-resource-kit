using Xunit;
using System.Collections.Generic;
using apimtemplate.Common.Templates.Abstractions;
using apimtemplate.Creator.TemplateCreators;
using apimtemplate.Creator.Models;
using apimtemplate.Common.TemplateModels;

namespace apimtemplate.test.Creator.TemplateCreatorTests
{
    public class APIVersionSetTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateAPIVersionSetTemplateFromCreatorConfig()
        {
            // arrange
            APIVersionSetTemplateCreator apiVersionSetTemplateCreator = new APIVersionSetTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { apiVersionSets = new List<APIVersionSetConfig>() };
            APIVersionSetConfig apiVersionSet = new APIVersionSetConfig()
            {
                id = "id",
                description = "description",
                displayName = "displayName",
                versionHeaderName = "versionHeaderName",
                versioningScheme = "versioningScheme",
                versionQueryName = "versionQueryName"
            };
            creatorConfig.apiVersionSets.Add(apiVersionSet);

            // act
            Template versionSetTemplate = apiVersionSetTemplateCreator.CreateAPIVersionSetTemplate(creatorConfig);
            APIVersionSetTemplateResource apiVersionSetTemplateResource = (APIVersionSetTemplateResource)versionSetTemplate.resources[0];

            // assert
            Assert.Equal(apiVersionSet.description, apiVersionSetTemplateResource.properties.description);
            Assert.Equal(apiVersionSet.displayName, apiVersionSetTemplateResource.properties.displayName);
            Assert.Equal(apiVersionSet.versionHeaderName, apiVersionSetTemplateResource.properties.versionHeaderName);
            Assert.Equal(apiVersionSet.versioningScheme, apiVersionSetTemplateResource.properties.versioningScheme);
            Assert.Equal(apiVersionSet.versionQueryName, apiVersionSetTemplateResource.properties.versionQueryName);
        }

        [Fact]
        public void ShouldUseDefaultResourceNameWithoutProvidedId()
        {
            // arrange
            APIVersionSetTemplateCreator apiVersionSetTemplateCreator = new APIVersionSetTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { apiVersionSets = new List<APIVersionSetConfig>() };
            APIVersionSetConfig apiVersionSet = new APIVersionSetConfig();
            creatorConfig.apiVersionSets.Add(apiVersionSet);

            // act
            Template versionSetTemplate = apiVersionSetTemplateCreator.CreateAPIVersionSetTemplate(creatorConfig);
            APIVersionSetTemplateResource apiVersionSetTemplateResource = (APIVersionSetTemplateResource)versionSetTemplate.resources[0];

            // assert
            Assert.Equal("[concat(parameters('ApimServiceName'), '/versionset')]", apiVersionSetTemplateResource.name);
        }

        [Fact]
        public void ShouldUseProvidedIdInResourceName()
        {
            // arrange
            APIVersionSetTemplateCreator apiVersionSetTemplateCreator = new APIVersionSetTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { apiVersionSets = new List<APIVersionSetConfig>() };
            APIVersionSetConfig apiVersionSet = new APIVersionSetConfig()
            {
                id = "id"
            };
            creatorConfig.apiVersionSets.Add(apiVersionSet);

            // act
            Template versionSetTemplate = apiVersionSetTemplateCreator.CreateAPIVersionSetTemplate(creatorConfig);
            APIVersionSetTemplateResource apiVersionSetTemplateResource = (APIVersionSetTemplateResource)versionSetTemplate.resources[0];

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{apiVersionSet.id}')]", apiVersionSetTemplateResource.name);
        }
    }
}
