// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorFactories;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
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
                serviceUrl = "http://serviceUrl",
                suffix = "suffix",
                subscriptionRequired = true,
                authenticationSettings = new APITemplateAuthenticationSettings()
                {
                    OAuth2 = new APITemplateOAuth2()
                    {
                        AuthorizationServerId = "",
                        Scope = ""
                    },
                    Openid = new APITemplateOpenID()
                    {
                        OpenIdProviderId = "",
                        BearerTokenSendingMethods = new string[] { }
                    },
                    SubscriptionKeyRequired = true
                },
                openApiSpec = "https://petstore.swagger.io/v2/swagger.json",
                protocols = "https",
                isCurrent = true,
                type = "http"
            };
            creatorConfig.apis.Add(api);

            // act
            APITemplateResource apiTemplateResource = await apiTemplateCreator.CreateAPITemplateResourceAsync(api, true, false);

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.name}')]", apiTemplateResource.Name);
            Assert.Equal($"[parameters('{api.name}-ServiceUrl')]", apiTemplateResource.Properties.ServiceUrl);
            Assert.Equal(api.name, apiTemplateResource.Properties.DisplayName);
            Assert.Equal(api.apiVersion, apiTemplateResource.Properties.ApiVersion);
            Assert.Equal(api.apiVersionDescription, apiTemplateResource.Properties.ApiVersionDescription);
            Assert.Equal(api.type, apiTemplateResource.Properties.Type);
            Assert.Equal(api.type, apiTemplateResource.Properties.ApiType);
            Assert.Equal(api.isCurrent, apiTemplateResource.Properties.IsCurrent);
            Assert.Equal(new string[] { api.protocols }, apiTemplateResource.Properties.Protocols);
            Assert.Equal($"[resourceId('Microsoft.ApiManagement/service/apiVersionSets', parameters('{ParameterNames.ApimServiceName}'), '{api.apiVersionSetId}')]", apiTemplateResource.Properties.ApiVersionSetId);
            Assert.Equal(api.apiRevision, apiTemplateResource.Properties.ApiRevision);
            Assert.Equal(api.apiRevisionDescription, apiTemplateResource.Properties.ApiRevisionDescription);
            Assert.Equal(api.suffix, apiTemplateResource.Properties.Path);
            Assert.Equal(api.subscriptionRequired, apiTemplateResource.Properties.SubscriptionRequired);
            Assert.Equal(api.authenticationSettings.OAuth2.AuthorizationServerId, apiTemplateResource.Properties.AuthenticationSettings.OAuth2.AuthorizationServerId);
            Assert.Equal(api.authenticationSettings.OAuth2.Scope, apiTemplateResource.Properties.AuthenticationSettings.OAuth2.Scope);
            Assert.Equal(api.authenticationSettings.Openid.OpenIdProviderId, apiTemplateResource.Properties.AuthenticationSettings.Openid.OpenIdProviderId);
            Assert.Equal(api.authenticationSettings.Openid.BearerTokenSendingMethods, apiTemplateResource.Properties.AuthenticationSettings.Openid.BearerTokenSendingMethods);
            Assert.Equal(api.authenticationSettings.SubscriptionKeyRequired, apiTemplateResource.Properties.AuthenticationSettings.SubscriptionKeyRequired);
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
            APITemplateResource apiTemplateResource = await apiTemplateCreator.CreateAPITemplateResourceAsync(api, true, true);

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.name}')]", apiTemplateResource.Name);
            Assert.Equal("swagger-link-json", apiTemplateResource.Properties.Format);
            Assert.Equal(api.openApiSpec, apiTemplateResource.Properties.Value);
        }

        [Fact]
        public async void ShouldCreateSubsequentlAPITemplateResourceFromCreatorConfigWithAlternateTitle()
        {
            // arrange
            APITemplateCreator apiTemplateCreator = APITemplateCreatorFactory.GenerateAPITemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { apis = new List<APIConfig>() };
            APIConfig api = new APIConfig()
            {
                name = "name",
                displayName = "Swagger Petstore (alternate title)",
                openApiSpec = "https://petstore.swagger.io/v2/swagger.json",
            };
            creatorConfig.apis.Add(api);

            // act
            APITemplateResource apiTemplateResource = await apiTemplateCreator.CreateAPITemplateResourceAsync(api, true, true);

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.name}')]", apiTemplateResource.Name);
            Assert.Equal("swagger-json", apiTemplateResource.Properties.Format);

            // check alternate title has been specified in the embedded YAML or JSON definition

            var yaml = apiTemplateResource.Properties.Value;
            var deserializer = new YamlDotNet.Serialization.Deserializer();
            var definition = deserializer.Deserialize<Dictionary<string, object>>(yaml);
            var info = (Dictionary<object, object>)definition["info"];

            Assert.Equal("Swagger Petstore (alternate title)", info["title"]);
        }

        [Fact]
        public async void ShouldCreateSubsequentlAPITemplateResourceFromCreatorConfigWithAlternateTitleInSwagger()
        {
            // arrange
            APITemplateCreator apiTemplateCreator = APITemplateCreatorFactory.GenerateAPITemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { apis = new List<APIConfig>() };

            // extract swagger as a local file

            var swaggerPath = Path.GetTempFileName();
            using (var stream = new FileStream($"Resources{Path.DirectorySeparatorChar}OpenApiSpecs{Path.DirectorySeparatorChar}swaggerPetstorev3.json", FileMode.Open))
            using (var reader = new StreamReader(stream))
                File.WriteAllText(swaggerPath, reader.ReadToEnd());

            // create API config with local swagger definition

            APIConfig api = new APIConfig()
            {
                name = "name",
                displayName = "Swagger Petstore (alternate title)",
                openApiSpec = swaggerPath,
            };
            creatorConfig.apis.Add(api);

            // act
            APITemplateResource apiTemplateResource = await apiTemplateCreator.CreateAPITemplateResourceAsync(api, true, true);

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.name}')]", apiTemplateResource.Name);
            Assert.Equal("openapi+json", apiTemplateResource.Properties.Format);

            // check alternate title has been specified in the embedded YAML or JSON definition

            var yaml = apiTemplateResource.Properties.Value;
            var deserializer = new YamlDotNet.Serialization.Deserializer();
            var definition = deserializer.Deserialize<Dictionary<string, object>>(yaml);
            var info = (Dictionary<object, object>)definition["info"];

            Assert.Equal("Swagger Petstore (alternate title)", info["title"]);
        }

        [Fact]
        public async void ShouldCreateSubsequentlAPITemplateResourceFromCreatorConfigWithAlternateTitleInOpenApi()
        {
            // arrange
            APITemplateCreator apiTemplateCreator = APITemplateCreatorFactory.GenerateAPITemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { apis = new List<APIConfig>() };

            // extract swagger as a local file

            var openapiPath = Path.GetTempFileName();
            using (var stream = new FileStream($"Resources{Path.DirectorySeparatorChar}OpenApiSpecs{Path.DirectorySeparatorChar}swaggerPetstore.yml", FileMode.Open))
            using (var reader = new StreamReader(stream))
                File.WriteAllText(openapiPath, reader.ReadToEnd());

            // create API config with local swagger definition

            APIConfig api = new APIConfig()
            {
                name = "name",
                displayName = "Swagger Petstore (alternate title)",
                openApiSpec = openapiPath,
            };
            creatorConfig.apis.Add(api);

            // act
            APITemplateResource apiTemplateResource = await apiTemplateCreator.CreateAPITemplateResourceAsync(api, true, true);

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.name}')]", apiTemplateResource.Name);
            Assert.Equal("openapi", apiTemplateResource.Properties.Format);

            // check alternate title has been specified in the embedded YAML or JSON definition

            var yaml = apiTemplateResource.Properties.Value;
            var deserializer = new YamlDotNet.Serialization.Deserializer();
            var definition = deserializer.Deserialize<Dictionary<string, object>>(yaml);
            var info = (Dictionary<object, object>)definition["info"];

            Assert.Equal("Swagger Petstore (alternate title)", info["title"]);
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
                    OAuth2 = new APITemplateOAuth2()
                    {
                        AuthorizationServerId = "",
                        Scope = ""
                    },
                    Openid = new APITemplateOpenID()
                    {
                        OpenIdProviderId = "",
                        BearerTokenSendingMethods = new string[] { }
                    },
                    SubscriptionKeyRequired = true
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
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.name}')]", apiTemplateResource.Name);
            Assert.Equal(api.name, apiTemplateResource.Properties.DisplayName);
            Assert.Equal(api.apiVersion, apiTemplateResource.Properties.ApiVersion);
            Assert.Equal(api.type, apiTemplateResource.Properties.Type);
            Assert.Equal(api.type, apiTemplateResource.Properties.ApiType);
            Assert.Equal(api.isCurrent, apiTemplateResource.Properties.IsCurrent);
            Assert.Equal(new string[] { api.protocols }, apiTemplateResource.Properties.Protocols);
            Assert.Equal(api.apiVersionDescription, apiTemplateResource.Properties.ApiVersionDescription);
            Assert.Equal($"[resourceId('Microsoft.ApiManagement/service/apiVersionSets', parameters('{ParameterNames.ApimServiceName}'), '{api.apiVersionSetId}')]", apiTemplateResource.Properties.ApiVersionSetId);
            Assert.Equal(api.apiRevision, apiTemplateResource.Properties.ApiRevision);
            Assert.Equal(api.apiRevisionDescription, apiTemplateResource.Properties.ApiRevisionDescription);
            Assert.Equal(api.suffix, apiTemplateResource.Properties.Path);
            Assert.Equal(api.subscriptionRequired, apiTemplateResource.Properties.SubscriptionRequired);
            Assert.Equal(api.authenticationSettings.OAuth2.AuthorizationServerId, apiTemplateResource.Properties.AuthenticationSettings.OAuth2.AuthorizationServerId);
            Assert.Equal(api.authenticationSettings.OAuth2.Scope, apiTemplateResource.Properties.AuthenticationSettings.OAuth2.Scope);
            Assert.Equal(api.authenticationSettings.Openid.OpenIdProviderId, apiTemplateResource.Properties.AuthenticationSettings.Openid.OpenIdProviderId);
            Assert.Equal(api.authenticationSettings.Openid.BearerTokenSendingMethods, apiTemplateResource.Properties.AuthenticationSettings.Openid.BearerTokenSendingMethods);
            Assert.Equal(api.authenticationSettings.SubscriptionKeyRequired, apiTemplateResource.Properties.AuthenticationSettings.SubscriptionKeyRequired);
            Assert.Equal("swagger-link-json", apiTemplateResource.Properties.Format);
            Assert.Equal(api.openApiSpec, apiTemplateResource.Properties.Value);
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
                isCurrent = false,
                suffix = "suffix",
                subscriptionRequired = true,
                openApiSpec = "https://petstore.swagger.io/v2/swagger.json",
            };
            creatorConfig.apis.Add(api);

            // act
            // the above api config will create a unified api template with a single resource
            List<Template> apiTemplates = await apiTemplateCreator.CreateAPITemplatesAsync(api);
            APITemplateResource apiTemplateResource = apiTemplates.FirstOrDefault().Resources[0] as APITemplateResource;

            // assert
            Assert.Contains(";rev", apiTemplateResource.Name);
        }
    }

}
