// --------------------------------------------------------------------------
//  <copyright file="MockProductApisClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    public class MockServiceApisProductsApiClient
    {
        public const string TemplateType = "Microsoft.ApiManagement/service/products/apis";
       
        public static IProductsClient GetMockedApiClientWithDefaultValues()
        {
            var mockServiceApiProductsApiClient = new Mock<IProductsClient>(MockBehavior.Strict);

            mockServiceApiProductsApiClient
                .Setup(x => x.GetAllLinkedToApiAsync(It.IsAny<ExtractorParameters>(), It.IsAny<string>()))
                .ReturnsAsync((ExtractorParameters extractorParameters, string serviceApiName) => new List<ProductApiTemplateResource>
                {
                    new ProductApiTemplateResource
                    {
                        Name = serviceApiName,
                        Type = TemplateType,
                        Properties = new ProductApiProperties
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
