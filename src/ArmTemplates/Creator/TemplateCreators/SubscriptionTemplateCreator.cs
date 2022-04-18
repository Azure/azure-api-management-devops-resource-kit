// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class SubscriptionTemplateCreator
    {
        public SubscriptionsTemplateResource CreateSubscriptionsTemplateResource(SubscriptionConfig subscription, string[] dependsOn)
        {
            return new SubscriptionsTemplateResource
            {
                Name = $"[concat(parameters('ApimServiceName'), '/{subscription.name}')]",
                Type = "Microsoft.ApiManagement/service/subscriptions",
                ApiVersion = "2019-01-01",
                Properties = new SubscriptionsTemplateProperties
                {
                    ownerId = subscription.ownerId,
                    scope = subscription.scope,
                    displayName = subscription.displayName,
                    primaryKey = subscription.primaryKey,
                    secondaryKey = subscription.secondaryKey,
                    state = subscription.state,
                    allowTracing = subscription.allowTracing,
                },
                DependsOn = dependsOn,
            };
        }

        public List<SubscriptionsTemplateResource> CreateSubscriptionsTemplateResources(ProductConfig product, string[] dependsOn)
        {
            // TODO: throw error if scope is not null

            var scope = $"/products/{product.Name}";

            var resources = new List<SubscriptionsTemplateResource>(product.subscriptions.Count);

            foreach (var subscription in product.subscriptions)
            {
                subscription.scope = scope;
                resources.Add(this.CreateSubscriptionsTemplateResource(subscription, dependsOn));
            }

            return resources;
        }
    }
}