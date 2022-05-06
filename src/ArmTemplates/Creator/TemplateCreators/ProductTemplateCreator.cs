// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Groups;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class ProductTemplateCreator : IProductTemplateCreator
    {
        readonly ITemplateBuilder templateBuilder;
        readonly IPolicyTemplateCreator policyTemplateCreator;
        readonly IProductGroupTemplateCreator productGroupTemplateCreator;
        readonly ISubscriptionTemplateCreator subscriptionTemplateCreator;

        public ProductTemplateCreator(
            IPolicyTemplateCreator policyTemplateCreator,
            IProductGroupTemplateCreator productGroupTemplateCreator,
            ISubscriptionTemplateCreator subscriptionTemplateCreator,
            ITemplateBuilder templateBuilder)
        {
            this.policyTemplateCreator = policyTemplateCreator;
            this.productGroupTemplateCreator = productGroupTemplateCreator;
            this.subscriptionTemplateCreator = subscriptionTemplateCreator;
            this.templateBuilder = templateBuilder;
        }

        public Template CreateProductTemplate(CreatorParameters creatorConfig)
        {
            // create empty template
            Template productTemplate = this.templateBuilder.GenerateEmptyTemplate().Build();

            // add parameters
            productTemplate.Parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ Type = "string" } }
            };

            List<TemplateResource> resources = new List<TemplateResource>();
            foreach (ProductConfig product in creatorConfig.Products)
            {
                if (string.IsNullOrEmpty(product.Name))
                {
                    product.Name = product.DisplayName;
                }
                // create product resource with properties
                ProductsTemplateResource productsTemplateResource = new ProductsTemplateResource()
                {
                    Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{product.Name}')]",
                    Type = ResourceTypeConstants.Product,
                    ApiVersion = GlobalConstants.ApiVersion,
                    Properties = new ProductsProperties()
                    {
                        Description = product.Description,
                        Terms = product.Terms,
                        SubscriptionRequired = product.SubscriptionRequired,
                        ApprovalRequired = product.SubscriptionRequired ? product.ApprovalRequired : null,
                        SubscriptionsLimit = product.SubscriptionRequired ? product.SubscriptionsLimit : null,
                        State = product.State,
                        DisplayName = product.DisplayName
                    },
                    DependsOn = new string[] { }
                };
                resources.Add(productsTemplateResource);

                // create product policy resource that depends on the product, if provided
                if (product.Policy != null)
                {
                    string[] dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/products', parameters('{ParameterNames.ApimServiceName}'), '{product.Name}')]" };
                    PolicyTemplateResource productPolicy = this.policyTemplateCreator.CreateProductPolicyTemplateResource(product, dependsOn);
                    resources.Add(productPolicy);
                }

                // create product group resources if provided
                if (product.Groups != null)
                {
                    string[] dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/products', parameters('{ParameterNames.ApimServiceName}'), '{product.Name}')]" };
                    List<GroupTemplateResource> productGroups = this.productGroupTemplateCreator.CreateProductGroupTemplateResources(product, dependsOn);
                    resources.AddRange(productGroups);
                }

                // create product subscriptions if provided
                if (product.Subscriptions != null)
                {
                    string[] dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/products', parameters('{ParameterNames.ApimServiceName}'), '{product.Name}')]" };
                    List<SubscriptionsTemplateResource> subscriptions = this.subscriptionTemplateCreator.CreateSubscriptionsTemplateResources(product, dependsOn);
                    resources.AddRange(subscriptions);
                }
            }

            productTemplate.Resources = resources.ToArray();
            return productTemplate;
        }
    }
}