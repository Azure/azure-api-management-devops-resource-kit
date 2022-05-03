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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class APITemplateCreatorTests
    {
        [Fact]
        public async void ShouldCreateInitialAPITemplateResourceFromCreatorConfig()
        {
            // arrange
            var apiTemplateCreator = APITemplateCreatorFactory.GenerateAPITemplateCreator();
            CreatorParameters creatorConfig = new CreatorParameters() { Apis = new List<ApiConfig>() };
            ApiConfig api = new ApiConfig()
            {
                Name = "name",
                ApiVersion = "apiVersion",
                ApiVersionDescription = "apiVersionDescription",
                ApiVersionSetId = "apiVersionSetId",
                ApiRevision = "revision",
                ApiRevisionDescription = "revisionDescription",
                ServiceUrl = "http://serviceUrl",
                Suffix = "suffix",
                SubscriptionRequired = true,
                AuthenticationSettings = new APITemplateAuthenticationSettings()
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
                OpenApiSpec = "https://petstore.swagger.io/v2/swagger.json",
                Protocols = "https",
                IsCurrent = true,
                Type = "http"
            };
            creatorConfig.Apis.Add(api);

            // act
            APITemplateResource apiTemplateResource = await apiTemplateCreator.CreateAPITemplateResourceAsync(api, true, false);

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.Name}')]", apiTemplateResource.Name);
            Assert.Equal($"[parameters('{api.Name}-ServiceUrl')]", apiTemplateResource.Properties.ServiceUrl);
            Assert.Equal(api.Name, apiTemplateResource.Properties.DisplayName);
            Assert.Equal(api.ApiVersion, apiTemplateResource.Properties.ApiVersion);
            Assert.Equal(api.ApiVersionDescription, apiTemplateResource.Properties.ApiVersionDescription);
            Assert.Equal(api.Type, apiTemplateResource.Properties.Type);
            Assert.Equal(api.Type, apiTemplateResource.Properties.ApiType);
            Assert.Equal(api.IsCurrent, apiTemplateResource.Properties.IsCurrent);
            Assert.Equal(new string[] { api.Protocols }, apiTemplateResource.Properties.Protocols);
            Assert.Equal($"[resourceId('Microsoft.ApiManagement/service/apiVersionSets', parameters('{ParameterNames.ApimServiceName}'), '{api.ApiVersionSetId}')]", apiTemplateResource.Properties.ApiVersionSetId);
            Assert.Equal(api.ApiRevision, apiTemplateResource.Properties.ApiRevision);
            Assert.Equal(api.ApiRevisionDescription, apiTemplateResource.Properties.ApiRevisionDescription);
            Assert.Equal(api.Suffix, apiTemplateResource.Properties.Path);
            Assert.Equal(api.SubscriptionRequired, apiTemplateResource.Properties.SubscriptionRequired);
            Assert.Equal(api.AuthenticationSettings.OAuth2.AuthorizationServerId, apiTemplateResource.Properties.AuthenticationSettings.OAuth2.AuthorizationServerId);
            Assert.Equal(api.AuthenticationSettings.OAuth2.Scope, apiTemplateResource.Properties.AuthenticationSettings.OAuth2.Scope);
            Assert.Equal(api.AuthenticationSettings.Openid.OpenIdProviderId, apiTemplateResource.Properties.AuthenticationSettings.Openid.OpenIdProviderId);
            Assert.Equal(api.AuthenticationSettings.Openid.BearerTokenSendingMethods, apiTemplateResource.Properties.AuthenticationSettings.Openid.BearerTokenSendingMethods);
            Assert.Equal(api.AuthenticationSettings.SubscriptionKeyRequired, apiTemplateResource.Properties.AuthenticationSettings.SubscriptionKeyRequired);
        }

        [Fact]
        public async void ShouldCreateSubsequentlAPITemplateResourceFromCreatorConfigWithCorrectContent()
        {
            // arrange
            var apiTemplateCreator = APITemplateCreatorFactory.GenerateAPITemplateCreator();
            CreatorParameters creatorConfig = new CreatorParameters() { Apis = new List<ApiConfig>() };
            ApiConfig api = new ApiConfig()
            {
                Name = "name",
                OpenApiSpec = "https://petstore.swagger.io/v2/swagger.json"
            };
            creatorConfig.Apis.Add(api);

            // act
            APITemplateResource apiTemplateResource = await apiTemplateCreator.CreateAPITemplateResourceAsync(api, true, true);

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.Name}')]", apiTemplateResource.Name);
            Assert.Equal("swagger-link-json", apiTemplateResource.Properties.Format);
            Assert.Equal(api.OpenApiSpec, apiTemplateResource.Properties.Value);
        }

        [Fact]
        public async void ShouldCreateSubsequentlAPITemplateResourceFromCreatorConfigWithAlternateTitle()
        {
            // arrange
            var apiTemplateCreator = APITemplateCreatorFactory.GenerateAPITemplateCreator();
            CreatorParameters creatorConfig = new CreatorParameters() { Apis = new List<ApiConfig>() };
            ApiConfig api = new ApiConfig()
            {
                Name = "name",
                DisplayName = "Swagger Petstore (alternate title)",
                OpenApiSpec = "https://petstore.swagger.io/v2/swagger.json",
            };
            creatorConfig.Apis.Add(api);

            // act
            APITemplateResource apiTemplateResource = await apiTemplateCreator.CreateAPITemplateResourceAsync(api, true, true);

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.Name}')]", apiTemplateResource.Name);
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
            var apiTemplateCreator = APITemplateCreatorFactory.GenerateAPITemplateCreator();
            CreatorParameters creatorConfig = new CreatorParameters() { Apis = new List<ApiConfig>() };

            // extract swagger as a local file

            var swaggerPath = Path.GetTempFileName();
            using (var stream = new FileStream($"Resources{Path.DirectorySeparatorChar}OpenApiSpecs{Path.DirectorySeparatorChar}swaggerPetstorev3.json", FileMode.Open))
            using (var reader = new StreamReader(stream))
                File.WriteAllText(swaggerPath, reader.ReadToEnd());

            // create API config with local swagger definition

            ApiConfig api = new ApiConfig()
            {
                Name = "name",
                DisplayName = "Swagger Petstore (alternate title)",
                OpenApiSpec = swaggerPath,
            };
            creatorConfig.Apis.Add(api);

            // act
            APITemplateResource apiTemplateResource = await apiTemplateCreator.CreateAPITemplateResourceAsync(api, true, true);

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.Name}')]", apiTemplateResource.Name);
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
            var apiTemplateCreator = APITemplateCreatorFactory.GenerateAPITemplateCreator();
            CreatorParameters creatorConfig = new CreatorParameters() { Apis = new List<ApiConfig>() };

            // extract swagger as a local file

            var openapiPath = Path.GetTempFileName();
            using (var stream = new FileStream($"Resources{Path.DirectorySeparatorChar}OpenApiSpecs{Path.DirectorySeparatorChar}swaggerPetstore.yml", FileMode.Open))
            using (var reader = new StreamReader(stream))
                File.WriteAllText(openapiPath, reader.ReadToEnd());

            // create API config with local swagger definition

            ApiConfig api = new ApiConfig()
            {
                Name = "name",
                DisplayName = "Swagger Petstore (alternate title)",
                OpenApiSpec = openapiPath,
            };
            creatorConfig.Apis.Add(api);

            // act
            APITemplateResource apiTemplateResource = await apiTemplateCreator.CreateAPITemplateResourceAsync(api, true, true);

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.Name}')]", apiTemplateResource.Name);
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
            var apiTemplateCreator = APITemplateCreatorFactory.GenerateAPITemplateCreator();
            CreatorParameters creatorConfig = new CreatorParameters() { Apis = new List<ApiConfig>() };
            ApiConfig api = new ApiConfig()
            {
                Name = "name",
                ApiVersion = "apiVersion",
                ApiVersionDescription = "apiVersionDescription",
                ApiVersionSetId = "apiVersionSetId",
                ApiRevision = "revision",
                ApiRevisionDescription = "revisionDescription",
                Suffix = "suffix",
                SubscriptionRequired = true,
                AuthenticationSettings = new APITemplateAuthenticationSettings()
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
                OpenApiSpec = "https://petstore.swagger.io/v2/swagger.json",
                Protocols = "https",
                IsCurrent = true,
                Type = "http"
            };
            creatorConfig.Apis.Add(api);

            // act
            APITemplateResource apiTemplateResource = await apiTemplateCreator.CreateAPITemplateResourceAsync(api, false, true);

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.Name}')]", apiTemplateResource.Name);
            Assert.Equal(api.Name, apiTemplateResource.Properties.DisplayName);
            Assert.Equal(api.ApiVersion, apiTemplateResource.Properties.ApiVersion);
            Assert.Equal(api.Type, apiTemplateResource.Properties.Type);
            Assert.Equal(api.Type, apiTemplateResource.Properties.ApiType);
            Assert.Equal(api.IsCurrent, apiTemplateResource.Properties.IsCurrent);
            Assert.Equal(new string[] { api.Protocols }, apiTemplateResource.Properties.Protocols);
            Assert.Equal(api.ApiVersionDescription, apiTemplateResource.Properties.ApiVersionDescription);
            Assert.Equal($"[resourceId('Microsoft.ApiManagement/service/apiVersionSets', parameters('{ParameterNames.ApimServiceName}'), '{api.ApiVersionSetId}')]", apiTemplateResource.Properties.ApiVersionSetId);
            Assert.Equal(api.ApiRevision, apiTemplateResource.Properties.ApiRevision);
            Assert.Equal(api.ApiRevisionDescription, apiTemplateResource.Properties.ApiRevisionDescription);
            Assert.Equal(api.Suffix, apiTemplateResource.Properties.Path);
            Assert.Equal(api.SubscriptionRequired, apiTemplateResource.Properties.SubscriptionRequired);
            Assert.Equal(api.AuthenticationSettings.OAuth2.AuthorizationServerId, apiTemplateResource.Properties.AuthenticationSettings.OAuth2.AuthorizationServerId);
            Assert.Equal(api.AuthenticationSettings.OAuth2.Scope, apiTemplateResource.Properties.AuthenticationSettings.OAuth2.Scope);
            Assert.Equal(api.AuthenticationSettings.Openid.OpenIdProviderId, apiTemplateResource.Properties.AuthenticationSettings.Openid.OpenIdProviderId);
            Assert.Equal(api.AuthenticationSettings.Openid.BearerTokenSendingMethods, apiTemplateResource.Properties.AuthenticationSettings.Openid.BearerTokenSendingMethods);
            Assert.Equal(api.AuthenticationSettings.SubscriptionKeyRequired, apiTemplateResource.Properties.AuthenticationSettings.SubscriptionKeyRequired);
            Assert.Equal("swagger-link-json", apiTemplateResource.Properties.Format);
            Assert.Equal(api.OpenApiSpec, apiTemplateResource.Properties.Value);
        }

        [Fact]
        public async void ShouldAppendRevisionToAPIName()
        {
            // arrange
            var apiTemplateCreator = APITemplateCreatorFactory.GenerateAPITemplateCreator();
            CreatorParameters creatorConfig = new CreatorParameters() { Apis = new List<ApiConfig>() };
            ApiConfig api = new ApiConfig()
            {
                Name = "name",
                ApiRevision = "2",
                IsCurrent = false,
                Suffix = "suffix",
                SubscriptionRequired = true,
                OpenApiSpec = "https://petstore.swagger.io/v2/swagger.json",
            };
            creatorConfig.Apis.Add(api);

            // act
            // the above api config will create a unified api template with a single resource
            List<Template> apiTemplates = await apiTemplateCreator.CreateAPITemplatesAsync(api);
            APITemplateResource apiTemplateResource = apiTemplates.FirstOrDefault().Resources[0] as APITemplateResource;

            // assert
            Assert.Contains(";rev", apiTemplateResource.Name);
        }
    }

}
