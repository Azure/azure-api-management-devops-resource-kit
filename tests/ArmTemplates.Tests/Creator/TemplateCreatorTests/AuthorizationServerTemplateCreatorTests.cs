// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.AuthorizationServer;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class AuthorizationServerTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateAuthorizationServerTemplateFromCreatorConfig()
        {
            // arrange
            AuthorizationServerTemplateCreator authorizationServerTemplateCreator = new AuthorizationServerTemplateCreator(new TemplateBuilder());
            CreatorConfig creatorConfig = new CreatorConfig() { authorizationServers = new List<AuthorizationServerProperties>() };
            AuthorizationServerProperties authorizationServer = new AuthorizationServerProperties()
            {
                Description = "description",
                DisplayName = "displayName",
                AuthorizationEndpoint = "endpoint.com",
                AuthorizationMethods = new string[] { "GET" },
                TokenBodyParameters = new AuthorizationServerTokenBodyParameter[] { new AuthorizationServerTokenBodyParameter() {
                    Name = "name",
                    Value = "value"
                } },
                ClientAuthenticationMethod = new string[] { "GET" },
                TokenEndpoint = "endpoint.com",
                SupportState = true,
                DefaultScope = "defaultScope",
                BearerTokenSendingMethods = new string[] { "GET" },
                ClientId = "id",
                ClientSecret = "secret",
                ClientRegistrationEndpoint = "endpoint.com",
                ResourceOwnerPassword = "pass",
                ResourceOwnerUsername = "user",
                GrantTypes = new string[] { }
            };
            creatorConfig.authorizationServers.Add(authorizationServer);

            // act
            Template authorizationServerTemplate = authorizationServerTemplateCreator.CreateAuthorizationServerTemplate(creatorConfig);
            AuthorizationServerTemplateResource authorizationServerTemplateResource = (AuthorizationServerTemplateResource)authorizationServerTemplate.Resources[0];

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{authorizationServer.DisplayName}')]", authorizationServerTemplateResource.Name);
            Assert.Equal(authorizationServer.Description, authorizationServerTemplateResource.Properties.Description);
            Assert.Equal(authorizationServer.DisplayName, authorizationServerTemplateResource.Properties.DisplayName);
            Assert.Equal(authorizationServer.AuthorizationEndpoint, authorizationServerTemplateResource.Properties.AuthorizationEndpoint);
            Assert.Equal(authorizationServer.AuthorizationMethods, authorizationServerTemplateResource.Properties.AuthorizationMethods);
            Assert.Equal(authorizationServer.ClientAuthenticationMethod, authorizationServerTemplateResource.Properties.ClientAuthenticationMethod);
            Assert.Equal(authorizationServer.ClientId, authorizationServerTemplateResource.Properties.ClientId);
            Assert.Equal(authorizationServer.ClientRegistrationEndpoint, authorizationServerTemplateResource.Properties.ClientRegistrationEndpoint);
            Assert.Equal(authorizationServer.ClientSecret, authorizationServerTemplateResource.Properties.ClientSecret);
            Assert.Equal(authorizationServer.BearerTokenSendingMethods, authorizationServerTemplateResource.Properties.BearerTokenSendingMethods);
            Assert.Equal(authorizationServer.GrantTypes, authorizationServerTemplateResource.Properties.GrantTypes);
            Assert.Equal(authorizationServer.ResourceOwnerPassword, authorizationServerTemplateResource.Properties.ResourceOwnerPassword);
            Assert.Equal(authorizationServer.ResourceOwnerUsername, authorizationServerTemplateResource.Properties.ResourceOwnerUsername);
            Assert.Equal(authorizationServer.DefaultScope, authorizationServerTemplateResource.Properties.DefaultScope);
            Assert.Equal(authorizationServer.SupportState, authorizationServerTemplateResource.Properties.SupportState);
            Assert.Equal(authorizationServer.TokenBodyParameters[0].Name, authorizationServerTemplateResource.Properties.TokenBodyParameters[0].Name);
            Assert.Equal(authorizationServer.TokenBodyParameters[0].Value, authorizationServerTemplateResource.Properties.TokenBodyParameters[0].Value);
        }
    }
}
