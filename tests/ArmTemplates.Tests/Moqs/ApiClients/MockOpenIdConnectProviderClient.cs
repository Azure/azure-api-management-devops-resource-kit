// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.OpenIdConnectProviders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.OpenIdConnectProviders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.IdentityProviderClients
{
    public class MockOpenIdConnectProviderClient
    {
        public const string TemplateType = "Microsoft.ApiManagement/service/identityProviders";
        public const string ClientSecretDefaultValue = "clientSecretValue";
        public static OpenIdConnectProviderProperties GetMockedOpenIdConnectProviderProperties(string providerName)
        {
            return new OpenIdConnectProviderProperties
            {
                ClientId = "clientid",
                ClientSecret = null,
                Description = "Description"
            };
        }

        public static List<OpenIdConnectProviderResource> GenerateMockedIdentityProviderList(List<string> providerNames) 
        {
            var openIdConnectProviderList = new List<OpenIdConnectProviderResource>();
            foreach(var providerName in providerNames)
            {
                var properties = GetMockedOpenIdConnectProviderProperties(providerName);
                openIdConnectProviderList.Add(new OpenIdConnectProviderResource
                {
                    OriginalName = providerName,
                    Name = providerName,
                    Type = TemplateType,
                    Properties = properties
                });
            }

            return openIdConnectProviderList;
        }

        public static IOpenIdConnectProvidersClient GetMockedOpenIdConnectProviderClient(List<string> providerNames)
        {
            var mockServiceProviderApiClient = new Mock<IOpenIdConnectProvidersClient>(MockBehavior.Strict);

            mockServiceProviderApiClient
                .Setup(x => x.GetAllAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((ExtractorParameters _) => GenerateMockedIdentityProviderList(providerNames));

            mockServiceProviderApiClient
                .Setup(x => x.ListOpenIdConnectProviderSecretsAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((string _, ExtractorParameters _) => new OpenIdConnectProviderSecret() 
                {
                    ClientSecret = ClientSecretDefaultValue
                });

            return mockServiceProviderApiClient.Object;
        }

        public static async Task<IOpenIdConnectProvidersClient> GetMockedHttpOpenIdConnectProviderClient(string responseFileLocation)
        {
            var mockedProcessor = new Mock<IOpenIdConnectProviderProcessor>(MockBehavior.Loose).Object;
            var mockedClient = new Mock<OpenIdConnectProviderClient>(MockBehavior.Strict, await MockClientUtils.GenerateMockedIHttpClientFactoryWithResponse(responseFileLocation), mockedProcessor);
            MockClientUtils.MockAuthOfApiClient(mockedClient);

            return mockedClient.Object;
        }
    }
}
