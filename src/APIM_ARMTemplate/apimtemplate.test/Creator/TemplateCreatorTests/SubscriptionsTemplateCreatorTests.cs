using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
{
    public class SubscriptionTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateSubscriptionFromCreatorConfig()
        {
            // arrange
            SubscriptionTemplateCreator subscriptionTemplateCreator = new SubscriptionTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { subscriptions = new List<SubscriptionConfig>() };
            SubscriptionConfig subscription = new SubscriptionConfig()
            {
                ownerId = "ownerId",
                scope = "scope",
                displayName = "displayName",
                primaryKey = "primaryKey",
                secondaryKey = "secondaryKey",
                state = "state",
                allowTracing = true,
            };
            creatorConfig.subscriptions.Add(subscription);

            // act
            Template subscriptionTemplate = subscriptionTemplateCreator.CreateSubscriptionTemplate(creatorConfig);
            SubscriptionsTemplateResource subscriptionsTemplateResource = (SubscriptionsTemplateResource)subscriptionTemplate.resources[0];

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{subscription.displayName}')]", subscriptionsTemplateResource.name);
            Assert.Equal(subscription.scope, subscriptionsTemplateResource.properties.scope);
            Assert.Equal(subscription.displayName, subscriptionsTemplateResource.properties.displayName);
            Assert.Equal(subscription.primaryKey, subscriptionsTemplateResource.properties.primaryKey);
            Assert.Equal(subscription.secondaryKey, subscriptionsTemplateResource.properties.secondaryKey);
            Assert.Equal(subscription.state, subscriptionsTemplateResource.properties.state);
            Assert.Equal(subscription.allowTracing, subscriptionsTemplateResource.properties.allowTracing);
        }

    }
}