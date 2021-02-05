using System;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class SubscriptionTemplateCreator : TemplateCreator
    {
        public SubscriptionsTemplateResource CreateSubscriptionsTemplateResource(SubscriptionConfig subscription, string[] dependsOn)
        {
            return new SubscriptionsTemplateResource
            {
                name = $"[concat(parameters('ApimServiceName'), '/{subscription.name}')]",
                type = "Microsoft.ApiManagement/service/subscriptions",
                apiVersion = "2019-01-01",
                properties = new SubscriptionsTemplateProperties
                {
                    ownerId = subscription.ownerId,
                    scope = subscription.scope,
                    displayName = subscription.displayName,
                    primaryKey = subscription.primaryKey,
                    secondaryKey = subscription.secondaryKey,
                    state = subscription.state,
                    allowTracing = subscription.allowTracing,
                },
                dependsOn = dependsOn,
            };
        }

        public List<SubscriptionsTemplateResource> CreateSubscriptionsTemplateResources(ProductConfig product, string[] dependsOn)
        {
            // TODO: throw error if scope is not null

            var scope = $"/products/{product.name}";

            var resources = new List<SubscriptionsTemplateResource>(product.subscriptions.Count);

            foreach (var subscription in product.subscriptions)
            {
                subscription.scope = scope;
                resources.Add(CreateSubscriptionsTemplateResource(subscription, dependsOn));
            }

            return resources;
        }
    }
}