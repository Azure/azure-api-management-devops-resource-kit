using Xunit;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
{
    public class APITemplateCreatorTests
    {
        [Fact]
        public async void ShouldCreateInitialAPITemplateResourceFromCreatorConfig()
        {
            // arrange
            APITemplateCreator apiTemplateCreator = APITemplateCreatorFactory.GenerateAPITemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { apis = new List<APIConfig>() };
            APIConfig api = new APIConfig()
            {
                name = "name",
                apiVersion = "apiVersion",
                apiVersionDescription = "apiVersionDescription",
                apiVersionSetId = "apiVersionSetId",
                apiRevision = "revision",
                apiRevisionDescription = "revisionDescription",
                suffix = "suffix",
                subscriptionRequired = true,
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
                openApiSpec = "https://petstore.swagger.io/v2/swagger.json",
                protocols = "https",
                isCurrent = true,
                type = "http"
            };
            creatorConfig.apis.Add(api);

            // act
            APITemplateResource apiTemplateResource = await apiTemplateCreator.CreateAPITemplateResourceAsync(api, true, true);

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{api.name}')]", apiTemplateResource.name);
            Assert.Equal(api.name, apiTemplateResource.properties.displayName);
            Assert.Equal(api.apiVersion, apiTemplateResource.properties.apiVersion);
            Assert.Equal(api.apiVersionDescription, apiTemplateResource.properties.apiVersionDescription);
            Assert.Equal(api.type, apiTemplateResource.properties.type);
            Assert.Equal(api.type, apiTemplateResource.properties.apiType);
            Assert.Equal(api.isCurrent, apiTemplateResource.properties.isCurrent);
            Assert.Equal(new string[] { api.protocols }, apiTemplateResource.properties.protocols);
            Assert.Equal($"[resourceId('Microsoft.ApiManagement/service/apiVersionSets', parameters('ApimServiceName'), '{api.apiVersionSetId}')]", apiTemplateResource.properties.apiVersionSetId);
            Assert.Equal(api.apiRevision, apiTemplateResource.properties.apiRevision);
            Assert.Equal(api.apiRevisionDescription, apiTemplateResource.properties.apiRevisionDescription);
            Assert.Equal(api.suffix, apiTemplateResource.properties.path);
            Assert.Equal(api.subscriptionRequired, apiTemplateResource.properties.subscriptionRequired);
            Assert.Equal(api.authenticationSettings.oAuth2.authorizationServerId, apiTemplateResource.properties.authenticationSettings.oAuth2.authorizationServerId);
            Assert.Equal(api.authenticationSettings.oAuth2.scope, apiTemplateResource.properties.authenticationSettings.oAuth2.scope);
            Assert.Equal(api.authenticationSettings.openid.openidProviderId, apiTemplateResource.properties.authenticationSettings.openid.openidProviderId);
            Assert.Equal(api.authenticationSettings.openid.bearerTokenSendingMethods, apiTemplateResource.properties.authenticationSettings.openid.bearerTokenSendingMethods);
            Assert.Equal(api.authenticationSettings.subscriptionKeyRequired, apiTemplateResource.properties.authenticationSettings.subscriptionKeyRequired);
        }

        [Fact]
        public async void ShouldCreateSubsequentlAPITemplateResourceFromCreatorConfigWithCorrectContent()
        {
            // arrange
            APITemplateCreator apiTemplateCreator = APITemplateCreatorFactory.GenerateAPITemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { apis = new List<APIConfig>() };
            APIConfig api = new APIConfig()
            {
                name = "name",
                openApiSpec = "https://petstore.swagger.io/v2/swagger.json"
            };
            creatorConfig.apis.Add(api);

            // act
            APITemplateResource apiTemplateResource = await apiTemplateCreator.CreateAPITemplateResourceAsync(api, true, false);

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{api.name}')]", apiTemplateResource.name);
            Assert.Equal("swagger-link-json", apiTemplateResource.properties.format);
            Assert.Equal(api.openApiSpec, apiTemplateResource.properties.value);
        }

        [Fact]
        public async void ShouldCreateUnifiedAPITemplateResourceFromCreatorConfig()
        {
            // arrange
            APITemplateCreator apiTemplateCreator = APITemplateCreatorFactory.GenerateAPITemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { apis = new List<APIConfig>() };
            APIConfig api = new APIConfig()
            {
                name = "name",
                apiVersion = "apiVersion",
                apiVersionDescription = "apiVersionDescription",
                apiVersionSetId = "apiVersionSetId",
                apiRevision = "revision",
                apiRevisionDescription = "revisionDescription",
                suffix = "suffix",
                subscriptionRequired = true,
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
                openApiSpec = "https://petstore.swagger.io/v2/swagger.json",
                protocols = "https",
                isCurrent = true,
                type = "http"
            };
            creatorConfig.apis.Add(api);

            // act
            APITemplateResource apiTemplateResource = await apiTemplateCreator.CreateAPITemplateResourceAsync(api, false, true);

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{api.name}')]", apiTemplateResource.name);
            Assert.Equal(api.name, apiTemplateResource.properties.displayName);
            Assert.Equal(api.apiVersion, apiTemplateResource.properties.apiVersion);
            Assert.Equal(api.type, apiTemplateResource.properties.type);
            Assert.Equal(api.type, apiTemplateResource.properties.apiType);
            Assert.Equal(api.isCurrent, apiTemplateResource.properties.isCurrent);
            Assert.Equal(new string[] { api.protocols }, apiTemplateResource.properties.protocols);
            Assert.Equal(api.apiVersionDescription, apiTemplateResource.properties.apiVersionDescription);
            Assert.Equal($"[resourceId('Microsoft.ApiManagement/service/apiVersionSets', parameters('ApimServiceName'), '{api.apiVersionSetId}')]", apiTemplateResource.properties.apiVersionSetId);
            Assert.Equal(api.apiRevision, apiTemplateResource.properties.apiRevision);
            Assert.Equal(api.apiRevisionDescription, apiTemplateResource.properties.apiRevisionDescription);
            Assert.Equal(api.suffix, apiTemplateResource.properties.path);
            Assert.Equal(api.subscriptionRequired, apiTemplateResource.properties.subscriptionRequired);
            Assert.Equal(api.authenticationSettings.oAuth2.authorizationServerId, apiTemplateResource.properties.authenticationSettings.oAuth2.authorizationServerId);
            Assert.Equal(api.authenticationSettings.oAuth2.scope, apiTemplateResource.properties.authenticationSettings.oAuth2.scope);
            Assert.Equal(api.authenticationSettings.openid.openidProviderId, apiTemplateResource.properties.authenticationSettings.openid.openidProviderId);
            Assert.Equal(api.authenticationSettings.openid.bearerTokenSendingMethods, apiTemplateResource.properties.authenticationSettings.openid.bearerTokenSendingMethods);
            Assert.Equal(api.authenticationSettings.subscriptionKeyRequired, apiTemplateResource.properties.authenticationSettings.subscriptionKeyRequired);
            Assert.Equal("swagger-link-json", apiTemplateResource.properties.format);
            Assert.Equal(api.openApiSpec, apiTemplateResource.properties.value);
        }

        [Fact]
        public async void ShouldAppendRevisionToAPIName()
        {
            // arrange
            APITemplateCreator apiTemplateCreator = APITemplateCreatorFactory.GenerateAPITemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { apis = new List<APIConfig>() };
            APIConfig api = new APIConfig()
            {
                name = "name",
                apiRevision = "2",
                isCurrent = true,
                suffix = "suffix",
                subscriptionRequired = true,
                openApiSpec = "https://petstore.swagger.io/v2/swagger.json",
            };
            creatorConfig.apis.Add(api);

            // act
            // the above api config will create a unified api template with a single resource
            List<Template> apiTemplates = await apiTemplateCreator.CreateAPITemplatesAsync(api);
            APITemplateResource apiTemplateResource = apiTemplates.FirstOrDefault().resources[0] as APITemplateResource;

            // assert
            Assert.Contains(";rev", apiTemplateResource.name);
        }
    }

}
