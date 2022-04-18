// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    public class MockApisClient
    {
        public const string TemplateType = "Microsoft.ApiManagement/service/apis";
        
        public const string ServiceApiName1 = "api-name-1";
        public const string ServiceApiName2 = "api-name-2";

        public static ApiProperties ServiceApiProperties1 = new ApiProperties
        {
            DisplayName = "api-display-name-1",
            ApiRevision = "1",
            Description = "api-description-1",
            SubscriptionRequired = true,
            ServiceUrl = "https://azure-service-1-url.com",
            Path = "path-1",
            Protocols = new [] { "https" },
            IsCurrent = true
        };

        public static ApiProperties ServiceApiProperties2 = new ApiProperties
        {
            DisplayName = "api-display-name-2",
            ApiRevision = "2",
            Description = "api-description-2",
            SubscriptionRequired = true,
            ServiceUrl = "https://azure-service-2-url.com",
            Path = "path-2",
            Protocols = new[] { "https" },
            IsCurrent = true
        };

        public static IApisClient GetMockedApiClientWithDefaultValues()
        {
            var mockServiceApiProductsApiClient = new Mock<IApisClient>(MockBehavior.Strict);

            mockServiceApiProductsApiClient
                .Setup(x => x.GetAllAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new List<ApiTemplateResource>
                {
                    new ApiTemplateResource
                    {
                        Name = ServiceApiName1,
                        Type = TemplateType,
                        Properties = ServiceApiProperties1
                    },

                    new ApiTemplateResource
                    {
                        Name = ServiceApiName2,
                        Type = TemplateType,
                        Properties = ServiceApiProperties2
                    },
                });

            mockServiceApiProductsApiClient
                .Setup(x => x.GetAllLinkedToGatewayAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((string gatewayName, ExtractorParameters _) => new List<ApiTemplateResource>
                {
                    new ApiTemplateResource
                    {
                        Name = $"{gatewayName}-{ServiceApiName1}",
                        Type = TemplateType,
                        Properties = ServiceApiProperties1
                    },

                    new ApiTemplateResource
                    {
                        Name = $"{gatewayName}-{ServiceApiName2}",
                        Type = TemplateType,
                        Properties = ServiceApiProperties2
                    },
                });

            mockServiceApiProductsApiClient
                .Setup(x => x.GetSingleAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new ApiTemplateResource
                {
                    Name = ServiceApiName1,
                    Type = TemplateType,
                    Properties = ServiceApiProperties1
                });



            mockServiceApiProductsApiClient
                .Setup(x => x.GetAllLinkedToProductAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((string productName, ExtractorParameters _) => new List<ApiTemplateResource>
                {
                    new ApiTemplateResource
                    {
                        Name = ServiceApiName1,
                        Type = TemplateType,
                        Properties = ServiceApiProperties1
                    },

                    new ApiTemplateResource
                    {
                        Name = ServiceApiName2,
                        Type = TemplateType,
                        Properties = ServiceApiProperties2
                    }
                });

            return mockServiceApiProductsApiClient.Object;
        }
    }
}
