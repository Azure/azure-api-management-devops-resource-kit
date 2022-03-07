// --------------------------------------------------------------------------
//  <copyright file="MockProductApisClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    public class MockServiceApisProductsApiClient
    {
        public const string TemplateType = "Microsoft.ApiManagement/service/products/apis";
       
        public static IServiceApiProductsApiClient GetMockedApiClientWithDefaultValues()
        {
            var mockServiceApiProductsApiClient = new Mock<IServiceApiProductsApiClient>(MockBehavior.Strict);

            mockServiceApiProductsApiClient
                .Setup(x => x.GetServiceApiProductsAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync((string apimInstanceName, string resourceGroup, string serviceApiName) => new List<ServiceApisProductTemplateResource>
                {
                    new ServiceApisProductTemplateResource
                    {
                        Name = serviceApiName,
                        Type = TemplateType,
                        Properties = new ServiceApiProductProperties
                        {
                            DisplayName = serviceApiName,
                            Description = serviceApiName
                        }
                    }
                });

            return mockServiceApiProductsApiClient.Object;
        }
    }
}
