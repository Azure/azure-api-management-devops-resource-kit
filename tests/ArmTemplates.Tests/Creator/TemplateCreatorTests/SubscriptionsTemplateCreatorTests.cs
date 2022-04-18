// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class SubscriptionTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateSubscriptionResourceFromSubscriptionConfig()
        {
            // arrange
            SubscriptionTemplateCreator subscriptionTemplateCreator = new SubscriptionTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { subscriptions = new List<SubscriptionConfig>() };

            SubscriptionConfig subscription = new SubscriptionConfig()
            {
                name = "subscriptionName",
                ownerId = "user/ownerId",
                scope = "/products/productId",
                displayName = "displayName",
                primaryKey = "primaryKey",
                secondaryKey = "secondaryKey",
                state = "active",
                allowTracing = true,
            };

            string[] dependsOn = new string[] { "dependsOn" };

            // act
            var subscriptionsTemplateResource = subscriptionTemplateCreator.CreateSubscriptionsTemplateResource(subscription, dependsOn);

            // assert

            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{subscription.name}')]", subscriptionsTemplateResource.Name);
            Assert.Equal($"Microsoft.ApiManagement/service/subscriptions", subscriptionsTemplateResource.Type);
            Assert.Equal("2019-01-01", subscriptionsTemplateResource.ApiVersion);

            Assert.Equal(subscription.scope, subscriptionsTemplateResource.Properties.scope);
            Assert.Equal(subscription.displayName, subscriptionsTemplateResource.Properties.displayName);
            Assert.Equal(subscription.primaryKey, subscriptionsTemplateResource.Properties.primaryKey);
            Assert.Equal(subscription.secondaryKey, subscriptionsTemplateResource.Properties.secondaryKey);
            Assert.Equal(subscription.state, subscriptionsTemplateResource.Properties.state);
            Assert.Equal(subscription.allowTracing, subscriptionsTemplateResource.Properties.allowTracing);

            Assert.Equal(dependsOn, subscriptionsTemplateResource.DependsOn);
        }

        [Fact]
        public void ShouldCreateProductSubscriptionTemplateResourceFromCreatorConfigWithCorrectContent()
        {
            // arrange
            var subscriptionTemplateCreator = new SubscriptionTemplateCreator();

            CreatorConfig creatorConfig = new CreatorConfig() { products = new List<ProductConfig>() };
            ProductConfig product = new ProductConfig()
            {
                Name = "productName",
                DisplayName = "displayName",
                Description = "description",
                Terms = "terms",
                SubscriptionRequired = true,
                ApprovalRequired = true,
                SubscriptionsLimit = 1,
                State = "state",
            };

            SubscriptionConfig subscription = new SubscriptionConfig()
            {
                name = "subscriptionName",
                ownerId = "user/ownerId",
                displayName = "displayName",
                primaryKey = "primaryKey",
                secondaryKey = "secondaryKey",
                state = "active",
                allowTracing = true,
            };

            product.subscriptions = new List<SubscriptionConfig>();
            product.subscriptions.Add(subscription);

            creatorConfig.products.Add(product);

            var dependsOn = new[] { $"[resourceId('Microsoft.ApiManagement/service/products', parameters('ApimServiceName'), '{product.Name}')]" };

            // act
            var subscriptionsTemplateResources = subscriptionTemplateCreator.CreateSubscriptionsTemplateResources(product, dependsOn);

            // assert

            var subscriptionsTemplateResource = subscriptionsTemplateResources[0];

            Assert.Equal($"/products/{product.Name}", subscriptionsTemplateResource.Properties.scope);
            Assert.Equal(subscription.displayName, subscriptionsTemplateResource.Properties.displayName);
            Assert.Equal(subscription.primaryKey, subscriptionsTemplateResource.Properties.primaryKey);
            Assert.Equal(subscription.secondaryKey, subscriptionsTemplateResource.Properties.secondaryKey);
            Assert.Equal(subscription.state, subscriptionsTemplateResource.Properties.state);
            Assert.Equal(subscription.allowTracing, subscriptionsTemplateResource.Properties.allowTracing);
        }
    }
}