// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorFactories;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class ProductTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateProductFromCreatorConfig()
        {
            // arrange
            ProductTemplateCreator productTemplateCreator = ProductTemplateCreatorFactory.GenerateProductTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { products = new List<ProductConfig>() };
            ProductConfig product = new ProductConfig()
            {
                Name = "name",
                DisplayName = "display name",
                Description = "description",
                Terms = "terms",
                SubscriptionRequired = true,
                ApprovalRequired = true,
                SubscriptionsLimit = 1,
                State = "state"
            };
            creatorConfig.products.Add(product);

            // act
            var productTemplate = productTemplateCreator.CreateProductTemplate(creatorConfig);
            var productsTemplateResource = (ProductsTemplateResource)productTemplate.Resources[0];

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{product.Name}')]", productsTemplateResource.Name);
            Assert.Equal(product.DisplayName, productsTemplateResource.Properties.DisplayName);
            Assert.Equal(product.Description, productsTemplateResource.Properties.Description);
            Assert.Equal(product.Terms, productsTemplateResource.Properties.Terms);
            Assert.Equal(product.SubscriptionsLimit, productsTemplateResource.Properties.SubscriptionsLimit);
            Assert.Equal(product.SubscriptionRequired, productsTemplateResource.Properties.SubscriptionRequired);
            Assert.Equal(product.ApprovalRequired, productsTemplateResource.Properties.ApprovalRequired);
            Assert.Equal(product.State, productsTemplateResource.Properties.State);
        }

        [Fact]
        public void ShouldNotCreateApprovalRequiredOrSubscriptionsLimitIfSubscriptionRequiredIsFalse()
        {
            // arrange
            ProductTemplateCreator productTemplateCreator = ProductTemplateCreatorFactory.GenerateProductTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { products = new List<ProductConfig>() };
            ProductConfig product = new ProductConfig()
            {
                DisplayName = "displayName",
                Description = "description",
                Terms = "terms",
                SubscriptionRequired = false,
                ApprovalRequired = true,
                SubscriptionsLimit = 1,
                State = "state"
            };
            creatorConfig.products.Add(product);

            // act
            var productTemplate = productTemplateCreator.CreateProductTemplate(creatorConfig);
            ProductsTemplateResource productsTemplateResource = (ProductsTemplateResource)productTemplate.Resources[0];

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{product.Name}')]", productsTemplateResource.Name);
            Assert.Equal(product.DisplayName, productsTemplateResource.Properties.DisplayName);
            Assert.Equal(product.Description, productsTemplateResource.Properties.Description);
            Assert.Equal(product.Terms, productsTemplateResource.Properties.Terms);
            Assert.Equal(product.SubscriptionRequired, productsTemplateResource.Properties.SubscriptionRequired);
            Assert.Null(productsTemplateResource.Properties.SubscriptionsLimit);
            Assert.Null(productsTemplateResource.Properties.ApprovalRequired);
            Assert.Equal(product.State, productsTemplateResource.Properties.State);
        }
    }
}
