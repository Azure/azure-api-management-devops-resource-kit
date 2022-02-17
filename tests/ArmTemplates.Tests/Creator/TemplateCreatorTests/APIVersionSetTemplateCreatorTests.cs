using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
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
            APIVersionSetTemplateResource apiVersionSetTemplateResource = (APIVersionSetTemplateResource)versionSetTemplate.Resources[0];

            // assert
            Assert.Equal(apiVersionSet.description, apiVersionSetTemplateResource.Properties.description);
            Assert.Equal(apiVersionSet.displayName, apiVersionSetTemplateResource.Properties.displayName);
            Assert.Equal(apiVersionSet.versionHeaderName, apiVersionSetTemplateResource.Properties.versionHeaderName);
            Assert.Equal(apiVersionSet.versioningScheme, apiVersionSetTemplateResource.Properties.versioningScheme);
            Assert.Equal(apiVersionSet.versionQueryName, apiVersionSetTemplateResource.Properties.versionQueryName);
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
            APIVersionSetTemplateResource apiVersionSetTemplateResource = (APIVersionSetTemplateResource)versionSetTemplate.Resources[0];

            // assert
            Assert.Equal("[concat(parameters('ApimServiceName'), '/versionset')]", apiVersionSetTemplateResource.Name);
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
            APIVersionSetTemplateResource apiVersionSetTemplateResource = (APIVersionSetTemplateResource)versionSetTemplate.Resources[0];

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{apiVersionSet.id}')]", apiVersionSetTemplateResource.Name);
        }
    }
}
