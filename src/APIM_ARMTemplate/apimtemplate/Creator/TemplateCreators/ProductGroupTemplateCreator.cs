using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class ProductGroupTemplateCreator
    {
        public ProductGroupsValue CreateProductGroupTemplateResource(string groupName, string productName, string[] dependsOn)
        {
            // create products/apis resource with properties
            ProductGroupsValue productAPITemplateResource = new ProductGroupsValue()
            {
                name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{productName}/{groupName}')]",
                type = ResourceTypeConstants.ProductGroup,
                apiVersion = GlobalConstants.APIVersion,
                dependsOn = dependsOn
            };
            return productGroupTemplateResource;
        }

        public List<ProductGroupsValue> CreateProductGroupTemplateResources(ProductConfig product, string[] dependsOn)
        {
            // create a products/apis association resource for each product provided in the config file
            List<ProductGroupsValue> productGroupTemplates = new List<ProductGroupsValue>();
            // products is comma separated list of productIds
            string[] groupNames = product.groups.Split(", ");
            foreach (string groupName in groupNames)
            {
                ProductGroupsValue productAPITemplate = this.CreateProductGroupTemplateResource(groupName, api.name, dependsOn);
                productGroupTemplates.Add(productAPITemplate);
            }
            return productGroupTemplates;
        }
    }
}
