using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class ProductTemplateCreator : TemplateCreator
    {
        private PolicyTemplateCreator policyTemplateCreator;
        private ProductGroupTemplateCreator productGroupTemplateCreator;

        public ProductTemplateCreator(PolicyTemplateCreator policyTemplateCreator, ProductGroupTemplateCreator productGroupTemplateCreator)
        {
            this.policyTemplateCreator = policyTemplateCreator;
            this.productGroupTemplateCreator = productGroupTemplateCreator;
        }

        public Template CreateProductTemplate(CreatorConfig creatorConfig)
        {
            // create empty template
            Template productTemplate = CreateEmptyTemplate();

            // add parameters
            productTemplate.parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ type = "string" } }
            };

            List<TemplateResource> resources = new List<TemplateResource>();
            foreach (ProductConfig product in creatorConfig.products)
            {
                if (string.IsNullOrEmpty(product.name)) {
                    product.name = product.displayName;
                }
                // create product resource with properties
                ProductsTemplateResource productsTemplateResource = new ProductsTemplateResource()
                {
                    name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{product.name}')]",
                    type = ResourceTypeConstants.Product,
                    apiVersion = GlobalConstants.APIVersion,
                    properties = new ProductsTemplateProperties()
                    {
                        description = product.description,
                        terms = product.terms,
                        subscriptionRequired = product.subscriptionRequired,
                        approvalRequired = product.subscriptionRequired ? product.approvalRequired : null,
                        subscriptionsLimit = product.subscriptionRequired ? product.subscriptionsLimit : null,
                        state = product.state,
                        displayName = product.displayName
                    },
                    dependsOn = new string[] { }
                };
                resources.Add(productsTemplateResource);

                // create product policy resource that depends on the product, if provided
                if (product.policy != null)
                {
                    string[] dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/products', parameters('{ParameterNames.ApimServiceName}'), '{product.name}')]" };
                    PolicyTemplateResource productPolicy = this.policyTemplateCreator.CreateProductPolicyTemplateResource(product, dependsOn);
                    resources.Add(productPolicy);
                }

                // create product group resources if provided
                if (product.groups != null)
                {
                    string[] dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/products', parameters('{ParameterNames.ApimServiceName}'), '{product.name}')]" };
                    List<ProductGroupsValue> productGroups = this.productGroupTemplateCreator.CreateProductGroupTemplateResources(product, dependsOn);
                    resources.AddRange(productGroups);
                }
            }

            productTemplate.resources = resources.ToArray();
            return productTemplate;
        }
    }
}
