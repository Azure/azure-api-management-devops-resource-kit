using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class ProductTemplateCreator : TemplateCreator
    {
        public Template CreateProductTemplate(CreatorConfig creatorConfig)
        {
            // create empty template
            Template productTemplate = CreateEmptyTemplate();

            // add parameters
            productTemplate.parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { "ApimServiceName", new TemplateParameterProperties(){ type = "string" } }
            };

            List<TemplateResource> resources = new List<TemplateResource>();
            foreach (ProductsTemplateProperties productTemplateProperties in creatorConfig.products)
            {
                // create product resource with properties
                ProductsTemplateResource productsTemplateResource = new ProductsTemplateResource()
                {
                    name = $"[concat(parameters('ApimServiceName'), '/{productTemplateProperties.displayName}')]",
                    type = ResourceTypeConstants.Product,
                    apiVersion = GlobalConstants.APIVersion,
                    properties = productTemplateProperties,
                    dependsOn = new string[] { }
                };
                resources.Add(productsTemplateResource);
            }

            productTemplate.resources = resources.ToArray();
            return productTemplate;
        }
    }
}
