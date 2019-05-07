using Xunit;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
{
    public class AuthorizationServerTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateAuthorizationServerTemplateFromCreatorConfig()
        {
            // arrange
            AuthorizationServerTemplateCreator authorizationServerTemplateCreator = new AuthorizationServerTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { authorizationServers = new List<AuthorizationServerTemplateProperties>() };
            AuthorizationServerTemplateProperties authorizationServer = new AuthorizationServerTemplateProperties()
            {
                description = "description",
                displayName = "displayName",
                authorizationEndpoint = "endpoint.com",
                authorizationMethods = new string[] { "GET" },
                tokenBodyParameters = new AuthorizationServerTokenBodyParameter[] { new AuthorizationServerTokenBodyParameter() {
                    name = "name",
                    value = "value"
                } },
                clientAuthenticationMethod = new string[] { "GET" },
                tokenEndpoint = "endpoint.com",
                supportState = true,
                defaultScope = "defaultScope",
                bearerTokenSendingMethods = new string[] { "GET" },
                clientId = "id",
                clientSecret = "secret",
                clientRegistrationEndpoint = "endpoint.com",
                resourceOwnerPassword = "pass",
                resourceOwnerUsername = "user",
                grantTypes = new string[] { }
            };
            creatorConfig.authorizationServers.Add(authorizationServer);

            // act
            Template authorizationServerTemplate = authorizationServerTemplateCreator.CreateAuthorizationServerTemplate(creatorConfig);
            AuthorizationServerTemplateResource authorizationServerTemplateResource = (AuthorizationServerTemplateResource)authorizationServerTemplate.resources[0];

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{authorizationServer.displayName}')]", authorizationServerTemplateResource.name);
            Assert.Equal(authorizationServer.description, authorizationServerTemplateResource.properties.description);
            Assert.Equal(authorizationServer.displayName, authorizationServerTemplateResource.properties.displayName);
            Assert.Equal(authorizationServer.authorizationEndpoint, authorizationServerTemplateResource.properties.authorizationEndpoint);
            Assert.Equal(authorizationServer.authorizationMethods, authorizationServerTemplateResource.properties.authorizationMethods);
            Assert.Equal(authorizationServer.clientAuthenticationMethod, authorizationServerTemplateResource.properties.clientAuthenticationMethod);
            Assert.Equal(authorizationServer.clientId, authorizationServerTemplateResource.properties.clientId);
            Assert.Equal(authorizationServer.clientRegistrationEndpoint, authorizationServerTemplateResource.properties.clientRegistrationEndpoint);
            Assert.Equal(authorizationServer.clientSecret, authorizationServerTemplateResource.properties.clientSecret);
            Assert.Equal(authorizationServer.bearerTokenSendingMethods, authorizationServerTemplateResource.properties.bearerTokenSendingMethods);
            Assert.Equal(authorizationServer.grantTypes, authorizationServerTemplateResource.properties.grantTypes);
            Assert.Equal(authorizationServer.resourceOwnerPassword, authorizationServerTemplateResource.properties.resourceOwnerPassword);
            Assert.Equal(authorizationServer.resourceOwnerUsername, authorizationServerTemplateResource.properties.resourceOwnerUsername);
            Assert.Equal(authorizationServer.defaultScope, authorizationServerTemplateResource.properties.defaultScope);
            Assert.Equal(authorizationServer.supportState, authorizationServerTemplateResource.properties.supportState);
            Assert.Equal(authorizationServer.tokenBodyParameters[0].name, authorizationServerTemplateResource.properties.tokenBodyParameters[0].name);
            Assert.Equal(authorizationServer.tokenBodyParameters[0].value, authorizationServerTemplateResource.properties.tokenBodyParameters[0].value);
        }
    }
}
