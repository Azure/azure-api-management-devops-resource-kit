// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.IdentityProviders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.IdentityProviderClients
{
    public class MockIdentityProviderClient
    {
        public const string TemplateType = "Microsoft.ApiManagement/service/identityProviders";
        public const string ClientSecretDefaultValue = "clientSecretValue";
        public static IdentityProviderProperties GetMockedIdentityProviderProperties()
        {
            return new IdentityProviderProperties
            {
                ClientId = "clientid",
                ClientSecret = null,
                Type = "facebook"
            };
        }

        public static List<IdentityProviderResource> GenerateMockedIdentityProviderList(List<string> identityProviderNames) 
        {
            var identityProviderList = new List<IdentityProviderResource>();
            foreach(var identityProvideName in identityProviderNames)
            {
                var properties = GetMockedIdentityProviderProperties();
                identityProviderList.Add(new IdentityProviderResource
                {
                    Name = identityProvideName,
                    Type = TemplateType,
                    Properties = properties
                });
            }

            return identityProviderList;
        }

        public static IIdentityProviderClient GetMockedIdentityProviderClient(List<string> identityProviderNames)
        {
            var mockServiceIdentityProviderApiClient = new Mock<IIdentityProviderClient>(MockBehavior.Strict);


            mockServiceIdentityProviderApiClient
                .Setup(x => x.GetAllAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((ExtractorParameters _) => GenerateMockedIdentityProviderList(identityProviderNames));

            mockServiceIdentityProviderApiClient
                .Setup(x => x.ListIdentityProviderSecrets(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((string _, ExtractorParameters _) => new IdentityProviderSecret() 
                {
                    ClientSecret = ClientSecretDefaultValue
                });

            return mockServiceIdentityProviderApiClient.Object;
        }
    }
}
