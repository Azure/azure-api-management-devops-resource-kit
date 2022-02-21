using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class ProductGroupTemplateCreator : TemplateGeneratorBase
    {
        public ProductGroupsValue CreateProductGroupTemplateResource(string groupName, string productName, string[] dependsOn)
        {
            // create products/apis resource with properties
            ProductGroupsValue productAPITemplateResource = new ProductGroupsValue()
            {
                Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{productName}/{groupName}')]",
                Type = ResourceTypeConstants.ProductGroup,
                ApiVersion = GlobalConstants.ApiVersion,
                DependsOn = dependsOn,
                Properties = new ProductGroupTemplateProperties()
            };
            return productAPITemplateResource;
        }

        public List<ProductGroupsValue> CreateProductGroupTemplateResources(ProductConfig product, string[] dependsOn)
        {
            // create a products/apis association resource for each product provided in the config file
            List<ProductGroupsValue> productGroupTemplates = new List<ProductGroupsValue>();
            // products is comma separated list of productIds
            string[] groupNames = product.groups.Split(", ");
            foreach (string groupName in groupNames)
            {
                ProductGroupsValue productAPITemplate = this.CreateProductGroupTemplateResource(groupName, product.name, dependsOn);
                productGroupTemplates.Add(productAPITemplate);
            }
            return productGroupTemplates;
        }
    }
}
