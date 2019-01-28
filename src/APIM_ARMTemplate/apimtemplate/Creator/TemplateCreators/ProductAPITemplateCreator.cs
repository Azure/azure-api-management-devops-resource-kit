using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class ProductAPITemplateCreator
    {
        public ProductAPITemplateResource CreateProductAPITemplateResource(string productID, string apiName, string[] dependsOn)
        {
            // create products/apis resource with properties
            ProductAPITemplateResource productAPITemplateResource = new ProductAPITemplateResource()
            {
                name = $"[concat(parameters('ApimServiceName'), '/{productID}/{apiName}')]",
                type = "Microsoft.ApiManagement/service/products/apis",
                apiVersion = "2018-01-01",
                properties = new ProductAPITemplateProperties(),
                dependsOn = dependsOn
            };
            return productAPITemplateResource;
        }

        public List<ProductAPITemplateResource> CreateProductAPITemplateResources(CreatorConfig creatorConfig, string[] dependsOn)
        {
            // create a products/apis association resource for each product provided in the config file
            List<ProductAPITemplateResource> productAPITemplates = new List<ProductAPITemplateResource>();
            string[] productIDs = creatorConfig.api.products.Split(", ");
            foreach (string productID in productIDs)
            {
                ProductAPITemplateResource productAPITemplate = this.CreateProductAPITemplateResource(productID, creatorConfig.api.name, dependsOn);
                productAPITemplates.Add(productAPITemplate);
            }
            return productAPITemplates;
        }
    }
}
