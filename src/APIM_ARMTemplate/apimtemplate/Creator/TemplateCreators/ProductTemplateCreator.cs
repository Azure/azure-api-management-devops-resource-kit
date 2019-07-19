using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class ProductTemplateCreator : TemplateCreator
    {
        private PolicyTemplateCreator policyTemplateCreator;

        public ProductTemplateCreator(PolicyTemplateCreator policyTemplateCreator)
        {
            this.policyTemplateCreator = policyTemplateCreator;
        }

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
            foreach (ProductConfig product in creatorConfig.products)
            {
                // create product resource with properties
                ProductsTemplateResource productsTemplateResource = new ProductsTemplateResource()
                {
                    name = $"[concat(parameters('ApimServiceName'), '/{product.name}')]",
                    type = ResourceTypeConstants.Product,
                    apiVersion = GlobalConstants.APIVersion,
                    properties = new ProductsTemplateProperties()
                    {
                        description = product.description,
                        terms = product.terms,
                        subscriptionRequired = product.subscriptionRequired,
                        approvalRequired = product.approvalRequired,
                        subscriptionsLimit = product.subscriptionsLimit,
                        state = product.state,
                        displayName = product.displayName
                    },
                    dependsOn = new string[] { }
                };
                resources.Add(productsTemplateResource);

                // create product policy resource that depends on the product, if provided
                if (product.policy != null)
                {
                    string[] dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/products', parameters('ApimServiceName'), '{product.name}')]" };
                    PolicyTemplateResource productPolicy = this.policyTemplateCreator.CreateProductPolicyTemplateResource(product, dependsOn);
                    resources.Add(productPolicy);
                }
            }

            productTemplate.resources = resources.ToArray();
            return productTemplate;
        }
    }
}
