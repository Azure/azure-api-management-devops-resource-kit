// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
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
            var productGroupTemplateResource = productgroupTemplateCreator.CreateProductGroupTemplateResource(groupName, productName, dependsOn);

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{productName}/{groupName}')]", productGroupTemplateResource.Name);
            Assert.Equal(dependsOn, productGroupTemplateResource.DependsOn);
        }

        [Fact]
        public void ShouldCreateCorrectNumberOfProductGroupTemplateResourcesFromCreatorConfig()
        {
            // arrange
            var productgroupTemplateCreator = new ProductGroupTemplateCreator();
            var config = new ProductConfig()
            {
                groups = "1, 2, 3"
            };
            int count = 3;
            string[] dependsOn = new string[] { "dependsOn" };

            // act
            var productAPITemplateResources = productgroupTemplateCreator.CreateProductGroupTemplateResources(config, dependsOn);

            // assert
            Assert.Equal(count, productAPITemplateResources.Count);

        }
    }
}
