using System.Collections.Generic;
using Xunit;
using apimtemplate.Common.TemplateModels;
using apimtemplate.Creator.Models;
using apimtemplate.Creator.TemplateCreators;

namespace apimtemplate.test.Creator.TemplateCreatorTests
{
    public class ProductGroupTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateProductGroupTemplateResourceFromValues()
        {
            // arrange
            ProductGroupTemplateCreator productgroupTemplateCreator = new ProductGroupTemplateCreator();
            string groupName = "groupName";
            string productName = "productName";
            string[] dependsOn = new string[] { "dependsOn" };

            // act
            ProductGroupsValue productGroupTemplateResource = productgroupTemplateCreator.CreateProductGroupTemplateResource(groupName, productName, dependsOn);

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{productName}/{groupName}')]", productGroupTemplateResource.name);
            Assert.Equal(dependsOn, productGroupTemplateResource.dependsOn);
        }

        [Fact]
        public void ShouldCreateCorrectNumberOfProductGroupTemplateResourcesFromCreatorConfig()
        {
            // arrange
            ProductGroupTemplateCreator productgroupTemplateCreator = new ProductGroupTemplateCreator();
            ProductConfig config = new ProductConfig()
            {
                groups = "1, 2, 3"
            };
            int count = 3;
            string[] dependsOn = new string[] { "dependsOn" };

            // act
            List<ProductGroupsValue> productAPITemplateResources = productgroupTemplateCreator.CreateProductGroupTemplateResources(config, dependsOn);

            // assert
            Assert.Equal(count, productAPITemplateResources.Count);

        }
    }
}
