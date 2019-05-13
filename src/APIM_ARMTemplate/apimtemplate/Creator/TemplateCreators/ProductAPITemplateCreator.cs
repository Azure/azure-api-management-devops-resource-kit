using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

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
                type = ResourceTypeConstants.ProductAPI,
                apiVersion = GlobalConstants.APIVersion,
                properties = new ProductAPITemplateProperties(),
                dependsOn = dependsOn
            };
            return productAPITemplateResource;
        }

        public List<ProductAPITemplateResource> CreateProductAPITemplateResources(APIConfig api, string[] dependsOn)
        {
            // create a products/apis association resource for each product provided in the config file
            List<ProductAPITemplateResource> productAPITemplates = new List<ProductAPITemplateResource>();
            // products is comma separated list of productIds
            string[] productIDs = api.products.Split(", ");
            foreach (string productID in productIDs)
            {
                ProductAPITemplateResource productAPITemplate = this.CreateProductAPITemplateResource(productID, api.name, dependsOn);
                productAPITemplates.Add(productAPITemplate);
            }
            return productAPITemplates;
        }
    }
}
