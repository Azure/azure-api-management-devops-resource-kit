using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorFactories;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;

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
                name = "name",
                displayName = "display name",
                description = "description",
                terms = "terms",
                subscriptionRequired = true,
                approvalRequired = true,
                subscriptionsLimit = 1,
                state = "state"
            };
            creatorConfig.products.Add(product);

            // act
            var productTemplate = productTemplateCreator.CreateProductTemplate(creatorConfig);
            var productsTemplateResource = (ProductsTemplateResource)productTemplate.Resources[0];

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{product.name}')]", productsTemplateResource.Name);
            Assert.Equal(product.displayName, productsTemplateResource.Properties.displayName);
            Assert.Equal(product.description, productsTemplateResource.Properties.description);
            Assert.Equal(product.terms, productsTemplateResource.Properties.terms);
            Assert.Equal(product.subscriptionsLimit, productsTemplateResource.Properties.subscriptionsLimit);
            Assert.Equal(product.subscriptionRequired, productsTemplateResource.Properties.subscriptionRequired);
            Assert.Equal(product.approvalRequired, productsTemplateResource.Properties.approvalRequired);
            Assert.Equal(product.state, productsTemplateResource.Properties.state);
        }

        [Fact]
        public void ShouldNotCreateApprovalRequiredOrSubscriptionsLimitIfSubscriptionRequiredIsFalse()
        {
            // arrange
            ProductTemplateCreator productTemplateCreator = ProductTemplateCreatorFactory.GenerateProductTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { products = new List<ProductConfig>() };
            ProductConfig product = new ProductConfig()
            {
                displayName = "displayName",
                description = "description",
                terms = "terms",
                subscriptionRequired = false,
                approvalRequired = true,
                subscriptionsLimit = 1,
                state = "state"
            };
            creatorConfig.products.Add(product);

            // act
            var productTemplate = productTemplateCreator.CreateProductTemplate(creatorConfig);
            ProductsTemplateResource productsTemplateResource = (ProductsTemplateResource)productTemplate.Resources[0];

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{product.name}')]", productsTemplateResource.Name);
            Assert.Equal(product.displayName, productsTemplateResource.Properties.displayName);
            Assert.Equal(product.description, productsTemplateResource.Properties.description);
            Assert.Equal(product.terms, productsTemplateResource.Properties.terms);
            Assert.Equal(product.subscriptionRequired, productsTemplateResource.Properties.subscriptionRequired);
            Assert.Null(productsTemplateResource.Properties.subscriptionsLimit);
            Assert.Null(productsTemplateResource.Properties.approvalRequired);
            Assert.Equal(product.state, productsTemplateResource.Properties.state);
        }
    }
}
