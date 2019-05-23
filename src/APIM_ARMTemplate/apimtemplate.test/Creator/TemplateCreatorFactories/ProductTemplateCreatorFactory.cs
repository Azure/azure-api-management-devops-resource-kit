using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
{
    public class ProductTemplateCreatorFactory
    {
        public static ProductTemplateCreator GenerateProductTemplateCreator()
        {
            PolicyTemplateCreator policyTemplateCreator = PolicyTemplateCreatorFactory.GeneratePolicyTemplateCreator();
            ProductTemplateCreator productTemplateCreator = new ProductTemplateCreator(policyTemplateCreator);
            return productTemplateCreator;
        }
    }
}
