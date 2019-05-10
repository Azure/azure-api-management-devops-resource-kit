using System.Collections.Generic;
using Xunit;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
{
    public class ProductAPITemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateProductAPITemplateResourceFromValues()
        {
            // arrange
            ProductAPITemplateCreator productAPITemplateCreator = new ProductAPITemplateCreator();
            string productId = "productId";
            string apiName = "apiName";
            string[] dependsOn = new string[] { "dependsOn" };

            // act
            ProductAPITemplateResource productAPITemplateResource = productAPITemplateCreator.CreateProductAPITemplateResource(productId, apiName, dependsOn);

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{productId}/{apiName}')]", productAPITemplateResource.name);
            Assert.Equal(dependsOn, productAPITemplateResource.dependsOn);
        }

        [Fact]
        public void ShouldCreateCorrectNumberOfProductAPITemplateResourcesFromCreatorConfig()
        {
            // arrange
            ProductAPITemplateCreator productAPITemplateCreator = new ProductAPITemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig();
            APIConfig api = new APIConfig()
            {
                products = "1, 2, 3"
            };
            int count = 3;
            string[] dependsOn = new string[] { "dependsOn" };

            // act
            List<ProductAPITemplateResource> productAPITemplateResources = productAPITemplateCreator.CreateProductAPITemplateResources(api, dependsOn);

            // assert
            Assert.Equal(count, productAPITemplateResources.Count);
        }
    }
}
