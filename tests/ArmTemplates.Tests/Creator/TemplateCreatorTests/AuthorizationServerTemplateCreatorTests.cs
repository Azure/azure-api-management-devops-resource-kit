using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
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
            AuthorizationServerTemplateResource authorizationServerTemplateResource = (AuthorizationServerTemplateResource)authorizationServerTemplate.Resources[0];

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{authorizationServer.displayName}')]", authorizationServerTemplateResource.Name);
            Assert.Equal(authorizationServer.description, authorizationServerTemplateResource.Properties.description);
            Assert.Equal(authorizationServer.displayName, authorizationServerTemplateResource.Properties.displayName);
            Assert.Equal(authorizationServer.authorizationEndpoint, authorizationServerTemplateResource.Properties.authorizationEndpoint);
            Assert.Equal(authorizationServer.authorizationMethods, authorizationServerTemplateResource.Properties.authorizationMethods);
            Assert.Equal(authorizationServer.clientAuthenticationMethod, authorizationServerTemplateResource.Properties.clientAuthenticationMethod);
            Assert.Equal(authorizationServer.clientId, authorizationServerTemplateResource.Properties.clientId);
            Assert.Equal(authorizationServer.clientRegistrationEndpoint, authorizationServerTemplateResource.Properties.clientRegistrationEndpoint);
            Assert.Equal(authorizationServer.clientSecret, authorizationServerTemplateResource.Properties.clientSecret);
            Assert.Equal(authorizationServer.bearerTokenSendingMethods, authorizationServerTemplateResource.Properties.bearerTokenSendingMethods);
            Assert.Equal(authorizationServer.grantTypes, authorizationServerTemplateResource.Properties.grantTypes);
            Assert.Equal(authorizationServer.resourceOwnerPassword, authorizationServerTemplateResource.Properties.resourceOwnerPassword);
            Assert.Equal(authorizationServer.resourceOwnerUsername, authorizationServerTemplateResource.Properties.resourceOwnerUsername);
            Assert.Equal(authorizationServer.defaultScope, authorizationServerTemplateResource.Properties.defaultScope);
            Assert.Equal(authorizationServer.supportState, authorizationServerTemplateResource.Properties.supportState);
            Assert.Equal(authorizationServer.tokenBodyParameters[0].name, authorizationServerTemplateResource.Properties.tokenBodyParameters[0].name);
            Assert.Equal(authorizationServer.tokenBodyParameters[0].value, authorizationServerTemplateResource.Properties.tokenBodyParameters[0].value);
        }
    }
}
