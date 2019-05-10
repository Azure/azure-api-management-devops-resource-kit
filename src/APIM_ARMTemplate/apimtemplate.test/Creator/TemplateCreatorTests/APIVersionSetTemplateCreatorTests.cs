using Xunit;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
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
