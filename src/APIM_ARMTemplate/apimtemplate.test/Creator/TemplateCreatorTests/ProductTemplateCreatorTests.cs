using Xunit;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
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
                displayName = "displayName",
                description = "description",
                terms = "terms",
                subscriptionRequired = true,
                approvalRequired = true,
                subscriptionsLimit = 1,
                state = "state"
            };
            creatorConfig.products.Add(product);

            // act
            Template productTemplate = productTemplateCreator.CreateProductTemplate(creatorConfig);
            ProductsTemplateResource productsTemplateResource = (ProductsTemplateResource)productTemplate.resources[0];

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{product.displayName}')]", productsTemplateResource.name);
            Assert.Equal(product.displayName, productsTemplateResource.properties.displayName);
            Assert.Equal(product.description, productsTemplateResource.properties.description);
            Assert.Equal(product.terms, productsTemplateResource.properties.terms);
            Assert.Equal(product.subscriptionsLimit, productsTemplateResource.properties.subscriptionsLimit);
            Assert.Equal(product.subscriptionRequired, productsTemplateResource.properties.subscriptionRequired);
            Assert.Equal(product.approvalRequired, productsTemplateResource.properties.approvalRequired);
            Assert.Equal(product.state, productsTemplateResource.properties.state);
        }
    }
}
