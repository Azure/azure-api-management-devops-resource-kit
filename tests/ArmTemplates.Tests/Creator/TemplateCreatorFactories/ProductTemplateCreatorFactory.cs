// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorFactories
{
    public class ProductTemplateCreatorFactory
    {
        public static ProductTemplateCreator GenerateProductTemplateCreator()
        {
            PolicyTemplateCreator policyTemplateCreator = PolicyTemplateCreatorFactory.GeneratePolicyTemplateCreator();
            ProductGroupTemplateCreator productGroupTemplateCreator = new ProductGroupTemplateCreator();
            SubscriptionTemplateCreator productSubscriptionsTemplateCreator = new SubscriptionTemplateCreator();
            ProductTemplateCreator productTemplateCreator = new ProductTemplateCreator(policyTemplateCreator, productGroupTemplateCreator, productSubscriptionsTemplateCreator, new TemplateBuilder());
            return productTemplateCreator;
        }
    }
}