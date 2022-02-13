using apimtemplate.Creator.TemplateCreators;

namespace apimtemplate.test.Creator.TemplateCreatorFactories
{
    public class ProductTemplateCreatorFactory
    {
        public static ProductTemplateCreator GenerateProductTemplateCreator()
        {
            PolicyTemplateCreator policyTemplateCreator = PolicyTemplateCreatorFactory.GeneratePolicyTemplateCreator();
            ProductGroupTemplateCreator productGroupTemplateCreator = new ProductGroupTemplateCreator();
            SubscriptionTemplateCreator productSubscriptionsTemplateCreator = new SubscriptionTemplateCreator();
            ProductTemplateCreator productTemplateCreator = new ProductTemplateCreator(policyTemplateCreator, productGroupTemplateCreator, productSubscriptionsTemplateCreator);
            return productTemplateCreator;
        }
    }
}