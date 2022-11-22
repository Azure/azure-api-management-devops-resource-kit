// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    public class MockApisClient
    {
        public const string TemplateType = "Microsoft.ApiManagement/service/apis";
        
        public const string ServiceApiName1 = "api-name-1";
        public const string ServiceApiName2 = "api-name-2";
        public const string ServiceApiName3 = "websocket-api";

        public static ApiProperties GetMockedServiceApiProperties2()
        { 
            return  new ApiProperties
            {
                DisplayName = "api-display-name-2",
                ApiRevision = "2",
                Description = "api-description-2",
                SubscriptionRequired = true,
                ServiceUrl = "https://azure-service-2-url.com",
                Path = "path-2",
                Protocols = new[] { "https" },
                IsCurrent = true,
                AuthenticationSettings = new ApiTemplateAuthenticationSettings
                {
                    OAuth2 = new ApiTemplateOAuth2
                    {
                        Scope = "scope-default-value-2",
                        AuthorizationServerId = "auth-server-id-1"
                    }
                }
            };
        }

        public static ApiProperties GetMockedServiceApiProperties1()
        {
            return new ApiProperties
            {
                DisplayName = "api-display-name-1",
                ApiRevision = "1",
                Description = "api-description-1",
                SubscriptionRequired = true,
                ServiceUrl = "https://azure-service-1-url.com",
                Path = "path-1",
                Protocols = new[] { "https" },
                IsCurrent = true
            };
        }

        public static ApiProperties GetMockedServiceApiPropertiesWebsocket()
        {
            return new ApiProperties
            {
                DisplayName = "websocket-api-display-name-3",
                ApiRevision = "1",
                Description = "api-description-3",
                SubscriptionRequired = true,
                ServiceUrl = "ws://host",
                Type = "websocket",
                Path = "path-3",
                IsCurrent = true,
            };
        }

        public static IApisClient GetMockedApiClientWithDefaultValues()
        {
            var mockedApisClient = new Mock<IApisClient>(MockBehavior.Strict);

            var serviceProperties1 = GetMockedServiceApiProperties1();
            var serviceProperties2 = GetMockedServiceApiProperties2();
            var serviceProperties3 = GetMockedServiceApiPropertiesWebsocket();

            mockedApisClient
                .Setup(x => x.GetAllAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((ExtractorParameters _) => new List<ApiTemplateResource>
                {
                    new ApiTemplateResource
                    {
                        Name = ServiceApiName1,
                        Type = TemplateType,
                        Properties = serviceProperties1
                    },

                    new ApiTemplateResource
                    {
                        Name = ServiceApiName2,
                        Type = TemplateType,
                        Properties = serviceProperties2
                    },

                    new ApiTemplateResource
                    {
                        Name = ServiceApiName3,
                        Type = TemplateType,
                        Properties = serviceProperties3
                    },
                });

            mockedApisClient
                .Setup(x => x.GetAllCurrentAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((ExtractorParameters _) => new List<ApiTemplateResource>
                {
                });

            mockedApisClient
                .Setup(x => x.GetAllLinkedToGatewayAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((string gatewayName, ExtractorParameters _) => new List<ApiTemplateResource>
                {
                    new ApiTemplateResource
                    {
                        Name = $"{gatewayName}-{ServiceApiName1}",
                        Type = TemplateType,
                        Properties = serviceProperties1
                    },

                    new ApiTemplateResource
                    {
                        Name = $"{gatewayName}-{ServiceApiName2}",
                        Type = TemplateType,
                        Properties = serviceProperties2
                    },
                });

            mockedApisClient
                .Setup(x => x.GetSingleAsync(It.Is<string>((o => o.Equals(ServiceApiName1))), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((string _, ExtractorParameters _) => new ApiTemplateResource
                {
                    Name = ServiceApiName1,
                    Type = TemplateType,
                    Properties = serviceProperties1
                });

            mockedApisClient
                .Setup(x => x.GetSingleAsync(It.Is<string>((o => o.Equals(ServiceApiName2))), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((string _, ExtractorParameters _) => new ApiTemplateResource
                {
                    Name = ServiceApiName2,
                    Type = TemplateType,
                    Properties = serviceProperties2
                });

            mockedApisClient
                .Setup(x => x.GetSingleAsync(It.Is<string>((o => o.Equals(ServiceApiName3))), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((string _, ExtractorParameters _) => new ApiTemplateResource
                {
                    Name = ServiceApiName3,
                    Type = TemplateType,
                    Properties = serviceProperties3
                });

            mockedApisClient
                .Setup(x => x.GetAllLinkedToProductAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((string productName, ExtractorParameters _) => new List<ApiTemplateResource>
                {
                    new ApiTemplateResource
                    {
                        Name = ServiceApiName1,
                        Type = TemplateType,
                        Properties = serviceProperties1
                    },

                    new ApiTemplateResource
                    {
                        Name = ServiceApiName2,
                        Type = TemplateType,
                        Properties = serviceProperties2
                    },

                    new ApiTemplateResource
                    {
                        Name = ServiceApiName3,
                        Type = TemplateType,
                        Properties = serviceProperties3
                    }
                });

            return mockedApisClient.Object;
        }

        public static async Task<IApisClient> GetMockedHttpApiClient(MockClientConfiguration mockClientConfiguration)
        {
            var apiDataProcessor = new ApiDataProcessor();
            var mockedClient = new Mock<ApisClient>(MockBehavior.Strict, await MockClientUtils.GenerateMockedIHttpClientFactoryWithResponse(mockClientConfiguration), apiDataProcessor);
            MockClientUtils.MockAuthOfApiClient(mockedClient);

            return mockedClient.Object;
        }
    }
}
