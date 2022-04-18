// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    public class MockProductsClient
    {
        public const string ProductName1 = "product-1";

        public static IProductsClient GetMockedApiClientWithDefaultValues()
        {
            var mockClient = new Mock<IProductsClient>(MockBehavior.Strict);

            mockClient
                .Setup(x => x.GetAllLinkedToApiAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((string serviceApiName, ExtractorParameters extractorParameters) => new List<ProductApiTemplateResource>
                {
                    new ProductApiTemplateResource
                    {
                        Name = serviceApiName,
                        Type = ResourceTypeConstants.ProductApi,
                        Properties = new ProductApiProperties
                        {
                            DisplayName = serviceApiName,
                            Description = serviceApiName
                        }
                    }
                });

            mockClient
                .Setup(x => x.GetAllAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new List<ProductsTemplateResource>
                {
                    new ProductsTemplateResource
                    {
                        Name = ProductName1,
                        Type = ResourceTypeConstants.Product,
                        Properties = new ProductsProperties
                        {
                            DisplayName = $"{ProductName1}-display",
                            Description = $"{ProductName1}-description"
                        }
                    }
                });

            return mockClient.Object;
        }
    }
}
