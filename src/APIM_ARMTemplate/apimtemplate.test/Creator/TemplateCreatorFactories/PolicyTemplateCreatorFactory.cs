
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class PolicyTemplateCreatorFactory
    {
        public static PolicyTemplateCreator GeneratePolicyTemplateCreator()
        {
            FileReader fileReader = new FileReader();
            PolicyTemplateCreator policyTemplateCreator = new PolicyTemplateCreator(fileReader);
            return policyTemplateCreator;
        }
    }
}
