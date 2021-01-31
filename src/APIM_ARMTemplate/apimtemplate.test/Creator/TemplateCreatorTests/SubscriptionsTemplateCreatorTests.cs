using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
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
            SubscriptionsTemplateResource subscriptionsTemplateResource = subscriptionTemplateCreator.CreateSubscriptionsTemplateResource(subscription, dependsOn);

            // assert

            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{subscription.name}')]", subscriptionsTemplateResource.name);
            Assert.Equal($"Microsoft.ApiManagement/service/subscriptions", subscriptionsTemplateResource.type);
            Assert.Equal("2019-01-01", subscriptionsTemplateResource.apiVersion);

            Assert.Equal(subscription.scope, subscriptionsTemplateResource.properties.scope);
            Assert.Equal(subscription.displayName, subscriptionsTemplateResource.properties.displayName);
            Assert.Equal(subscription.primaryKey, subscriptionsTemplateResource.properties.primaryKey);
            Assert.Equal(subscription.secondaryKey, subscriptionsTemplateResource.properties.secondaryKey);
            Assert.Equal(subscription.state, subscriptionsTemplateResource.properties.state);
            Assert.Equal(subscription.allowTracing, subscriptionsTemplateResource.properties.allowTracing);

            Assert.Equal(dependsOn, subscriptionsTemplateResource.dependsOn);
        }

        [Fact]
        public void ShouldCreateProductSubscriptionTemplateResourceFromCreatorConfigWithCorrectContent()
        {
            // arrange
            SubscriptionTemplateCreator subscriptionTemplateCreator = new SubscriptionTemplateCreator();

            CreatorConfig creatorConfig = new CreatorConfig() { products = new List<ProductConfig>() };
            ProductConfig product = new ProductConfig()
            {
                name = "productName",
                displayName = "displayName",
                description = "description",
                terms = "terms",
                subscriptionRequired = true,
                approvalRequired = true,
                subscriptionsLimit = 1,
                state = "state",
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

            var dependsOn = new[] { $"[resourceId('Microsoft.ApiManagement/service/products', parameters('ApimServiceName'), '{product.name}')]" };

            // act
            var subscriptionsTemplateResources = subscriptionTemplateCreator.CreateSubscriptionsTemplateResources(product, dependsOn);

            // assert

            var subscriptionsTemplateResource = subscriptionsTemplateResources[0];

            Assert.Equal($"/products/{product.name}", subscriptionsTemplateResource.properties.scope);
            Assert.Equal(subscription.displayName, subscriptionsTemplateResource.properties.displayName);
            Assert.Equal(subscription.primaryKey, subscriptionsTemplateResource.properties.primaryKey);
            Assert.Equal(subscription.secondaryKey, subscriptionsTemplateResource.properties.secondaryKey);
            Assert.Equal(subscription.state, subscriptionsTemplateResource.properties.state);
            Assert.Equal(subscription.allowTracing, subscriptionsTemplateResource.properties.allowTracing);
        }
    }
}