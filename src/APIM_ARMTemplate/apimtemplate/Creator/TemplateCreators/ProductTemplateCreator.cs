using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class ProductTemplateCreator
    {
        private TemplateCreator templateCreator;

        public ProductTemplateCreator(TemplateCreator templateCreator)
        {
            this.templateCreator = templateCreator;
        }

        public Template CreateProductTemplate(CreatorConfig creatorConfig)
        {
            // create empty template
            Template productTemplate = this.templateCreator.CreateEmptyTemplate();

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
                    apiVersion = "2018-06-01-preview",
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
