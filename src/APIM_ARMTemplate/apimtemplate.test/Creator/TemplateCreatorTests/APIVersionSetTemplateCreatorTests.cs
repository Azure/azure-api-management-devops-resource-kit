using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class APIVersionSetTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateAPIVersionSetTemplateFromCreatorConfig()
        {
            // arrange
            APIVersionSetTemplateCreator apiVersionSetTemplateCreator = APIVersionSetTemplateCreatorFactory.GenerateAPIVersionSetTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig()
            {
                apiVersionSet = new APIVersionSetConfig()
                {
                    id = "id",
                    description = "description",
                    displayName = "displayName",
                    versionHeaderName = "versionHeaderName",
                    versioningScheme = "versioningScheme",
                    versionQueryName = "versionQueryName"
                }
            };

            // act
            Template versionSetTemplate = apiVersionSetTemplateCreator.CreateAPIVersionSetTemplate(creatorConfig);
            APIVersionSetTemplateResource apiVersionSetTemplateResource = (APIVersionSetTemplateResource)versionSetTemplate.resources[0];

            // assert
            Assert.Equal(creatorConfig.apiVersionSet.description, apiVersionSetTemplateResource.properties.description);
            Assert.Equal(creatorConfig.apiVersionSet.displayName, apiVersionSetTemplateResource.properties.displayName);
            Assert.Equal(creatorConfig.apiVersionSet.versionHeaderName, apiVersionSetTemplateResource.properties.versionHeaderName);
            Assert.Equal(creatorConfig.apiVersionSet.versioningScheme, apiVersionSetTemplateResource.properties.versioningScheme);
            Assert.Equal(creatorConfig.apiVersionSet.versionQueryName, apiVersionSetTemplateResource.properties.versionQueryName);
        }

        [Fact]
        public void ShouldUseDefaultResourceNameWithoutProvidedId()
        {
            // arrange
            APIVersionSetTemplateCreator apiVersionSetTemplateCreator = APIVersionSetTemplateCreatorFactory.GenerateAPIVersionSetTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig()
            {
                apiVersionSet = new APIVersionSetConfig()                
            };

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
            APIVersionSetTemplateCreator apiVersionSetTemplateCreator = APIVersionSetTemplateCreatorFactory.GenerateAPIVersionSetTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig()
            {
                apiVersionSet = new APIVersionSetConfig()
                {
                    id = "id"
                }
            };

            // act
            Template versionSetTemplate = apiVersionSetTemplateCreator.CreateAPIVersionSetTemplate(creatorConfig);
            APIVersionSetTemplateResource apiVersionSetTemplateResource = (APIVersionSetTemplateResource)versionSetTemplate.resources[0];

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{creatorConfig.apiVersionSet.id}')]", apiVersionSetTemplateResource.name);
        }
    }
}
