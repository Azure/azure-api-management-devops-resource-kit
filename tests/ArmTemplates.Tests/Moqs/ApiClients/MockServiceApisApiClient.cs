// --------------------------------------------------------------------------
//  <copyright file="MockServiceApisApiClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Service;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    public class MockServiceApisApiClient
    {
        public const string TemplateType = "Microsoft.ApiManagement/service/apis";
        
        public const string ServiceApiName1 = "api-name-1";
        public const string ServiceApiName2 = "api-name-2";

        public static ServiceApiProperties ServiceApiProperties1 = new ServiceApiProperties
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

        public static ServiceApiProperties ServiceApiProperties2 = new ServiceApiProperties
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

        public static IServiceApisApiClient GetMockedApiClientWithDefaultValues()
        {
            var mockServiceApiProductsApiClient = new Mock<IServiceApisApiClient>(MockBehavior.Strict);

            mockServiceApiProductsApiClient
                .Setup(x => x.GetAllServiceApisAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new List<ServiceApiTemplateResource>
                {
                    new ServiceApiTemplateResource
                    {
                        Name = ServiceApiName1,
                        Type = TemplateType,
                        Properties = ServiceApiProperties1
                    },

                    new ServiceApiTemplateResource
                    {
                        Name = ServiceApiName2,
                        Type = TemplateType,
                        Properties = ServiceApiProperties2
                    },
                });

            mockServiceApiProductsApiClient
                .Setup(x => x.GetSingleServiceApiAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync(new ServiceApiTemplateResource
                {
                    Name = ServiceApiName1,
                    Type = TemplateType,
                    Properties = ServiceApiProperties1
                });

            return mockServiceApiProductsApiClient.Object;
        }
    }
}
