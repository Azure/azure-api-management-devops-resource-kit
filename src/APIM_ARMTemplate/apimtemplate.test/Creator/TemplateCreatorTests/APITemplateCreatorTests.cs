using Microsoft.OpenApi.Models;
using System;
using System.IO;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class APITemplateCreatorTests
    {
        [Fact]
        public async void ShouldCreateInitialAPITemplateResourceFromCreatorConfig()
        {
            // arrange
            APITemplateCreator apiTemplateCreator = APITemplateCreatorFactory.GenerateAPITemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig()
            {
                api = new APIConfig()
                {
                    name = "name",
                    apiVersion = "apiVersion",
                    apiVersionDescription = "apiVersionDescription",
                    apiVersionSetId = "apiVersionSetId",
                    revision = "revision",
                    revisionDescription = "revisionDescription",
                    suffix = "suffix",
                    authenticationSettings = new APITemplateAuthenticationSettings()
                    {
                        oAuth2 = new APITemplateOAuth2()
                        {
                            authorizationServerId = "",
                            scope = ""
                        },
                        openid = new APITemplateOpenID()
                        {
                            openidProviderId = "",
                            bearerTokenSendingMethods = new string[] { }
                        },
                        subscriptionKeyRequired = true
                    },
                    openApiSpec = "https://petstore.swagger.io/v2/swagger.json"
                }
            };

            // act
            APITemplateResource apiTemplateResource = await apiTemplateCreator.CreateInitialAPITemplateResourceAsync(creatorConfig);

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{creatorConfig.api.name}')]", apiTemplateResource.name);
            Assert.Equal(creatorConfig.api.name, apiTemplateResource.properties.displayName);
            Assert.Equal(creatorConfig.api.apiVersion, apiTemplateResource.properties.apiVersion);
            Assert.Equal(creatorConfig.api.apiVersionDescription, apiTemplateResource.properties.apiVersionDescription);
            Assert.Equal(creatorConfig.api.apiVersionSetId, apiTemplateResource.properties.apiVersionSetId);
            Assert.Equal(creatorConfig.api.revision, apiTemplateResource.properties.apiRevision);
            Assert.Equal(creatorConfig.api.revisionDescription, apiTemplateResource.properties.apiRevisionDescription);
            Assert.Equal(creatorConfig.api.suffix, apiTemplateResource.properties.path);
            Assert.Equal(creatorConfig.api.authenticationSettings.oAuth2.authorizationServerId, apiTemplateResource.properties.authenticationSettings.oAuth2.authorizationServerId);
            Assert.Equal(creatorConfig.api.authenticationSettings.oAuth2.scope, apiTemplateResource.properties.authenticationSettings.oAuth2.scope);
            Assert.Equal(creatorConfig.api.authenticationSettings.openid.openidProviderId, apiTemplateResource.properties.authenticationSettings.openid.openidProviderId);
            Assert.Equal(creatorConfig.api.authenticationSettings.openid.bearerTokenSendingMethods, apiTemplateResource.properties.authenticationSettings.openid.bearerTokenSendingMethods);
            Assert.Equal(creatorConfig.api.authenticationSettings.subscriptionKeyRequired, apiTemplateResource.properties.authenticationSettings.subscriptionKeyRequired);
        }

        [Fact]
        public async void ShouldUseDefaultVersionSetIdWithoutProvidedVersionSet()
        {
            // arrange
            APITemplateCreator apiTemplateCreator = APITemplateCreatorFactory.GenerateAPITemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig()
            {
                apiVersionSet = new APIVersionSetConfig()
                {

                },
                api = new APIConfig()
                {
                    openApiSpec = "https://petstore.swagger.io/v2/swagger.json"
                }
            };

            // act
            APITemplateResource apiTemplateResource = await apiTemplateCreator.CreateInitialAPITemplateResourceAsync(creatorConfig);

            // assert
            Assert.Equal("[resourceId('Microsoft.ApiManagement/service/api-version-sets', parameters('ApimServiceName'), 'versionset')]", apiTemplateResource.properties.apiVersionSetId);
        }

        [Fact]
        public void ShouldCreateSubsequentlAPITemplateResourceFromCreatorConfigWithCorrectContent()
        {
            // arrange
            APITemplateCreator apiTemplateCreator = APITemplateCreatorFactory.GenerateAPITemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig()
            {
                api = new APIConfig()
                {
                    name = "name",
                    openApiSpec = "https://petstore.swagger.io/v2/swagger.json"
                }
            };

            // act
            APITemplateResource apiTemplateResource = apiTemplateCreator.CreateSubsequentAPITemplateResource(creatorConfig);

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{creatorConfig.api.name}')]", apiTemplateResource.name);
            Assert.Equal("swagger-link-json", apiTemplateResource.properties.contentFormat);
            Assert.Equal(creatorConfig.api.openApiSpec, apiTemplateResource.properties.contentValue);
        }

        [Fact]
        public void ShouldCreateProtocolsFromOpenApiDocument()
        {
            // arrange
            APITemplateCreator apiTemplateCreator = APITemplateCreatorFactory.GenerateAPITemplateCreator();
            OpenApiDocument openApiDocument = new OpenApiDocument();
            int count = 2;
            for (int i = 0; i < count; i++)
            {
                openApiDocument.Servers.Add(new OpenApiServer()
                {
                    Url = $"{i}:{i}"
                });
            }

            // act
            string[] protocols = apiTemplateCreator.CreateProtocols(openApiDocument);

            // assert
            Assert.Equal(count, protocols.Length);
        }
    }
}
