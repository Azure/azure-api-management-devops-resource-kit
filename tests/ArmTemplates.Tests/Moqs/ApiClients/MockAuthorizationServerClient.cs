// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.AuthorizationServer;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    class MockAuthorizationServerClient
    {
        public const string AuthorizationServerName1 = "authorization-server-1";

        public static IAuthorizationServerClient GetMockedApiClientWithDefaultValues()
        {
            var mockAuthorizationServerClient = new Mock<IAuthorizationServerClient>(MockBehavior.Strict);

            mockAuthorizationServerClient
                .Setup(x => x.GetAllAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new List<AuthorizationServerTemplateResource>
                {
                    new AuthorizationServerTemplateResource
                    {
                        Name = AuthorizationServerName1,
                        Type = ResourceTypeConstants.AuthorizationServer,
                        Properties = new AuthorizationServerProperties
                        {
                            AuthorizationMethods = new [] { $"{AuthorizationServerName1}-auth-method" },
                            ClientAuthenticationMethod = new [] { $"{AuthorizationServerName1}-client-auth-method" },
                            TokenBodyParameters = new AuthorizationServerTokenBodyParameter[1],
                            TokenEndpoint = "http://example",
                            SupportState = false,
                            DisplayName = $"{AuthorizationServerName1}-display-name",
                            ClientRegistrationEndpoint = "http://example",
                            AuthorizationEndpoint = "http://example",
                            GrantTypes = new [] { "authorizationCode" },
                            ClientId = "client"
                        }
                    }
                });

            return mockAuthorizationServerClient.Object;
        }
    }
}
