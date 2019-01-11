using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class ProductAPITemplateCreator
    {
        public ProductAPITemplateResource CreateProductAPITemplateResource(string productID, string[] dependsOn)
        {
            // create products/apis resource with properties
            ProductAPITemplateResource productAPITemplateResource = new ProductAPITemplateResource()
            {
                name = $"[concat(parameters('ApimServiceName'), '/{productID}/api')]",
                type = "Microsoft.ApiManagement/service/products/apis",
                apiVersion = "2018-06-01-preview",
                properties = new ProductAPITemplateProperties(),
                dependsOn = dependsOn
            };
            return productAPITemplateResource;
        }

        public List<ProductAPITemplateResource> CreateProductAPITemplateResources(CreatorConfig creatorConfig, string[] dependsOn)
        {
            List<ProductAPITemplateResource> productAPITemplates = new List<ProductAPITemplateResource>();
            string[] productIDs = creatorConfig.api.products.Split(", ");
            foreach (string productID in productIDs)
            {
                ProductAPITemplateResource productAPITemplate = this.CreateProductAPITemplateResource(productID, dependsOn);
                productAPITemplates.Add(productAPITemplate);
            }
            return productAPITemplates;
        }
    }
}
