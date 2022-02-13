using apimtemplate.Common.FileHandlers;
using apimtemplate.Creator.TemplateCreators;

namespace apimtemplate.test.Creator.TemplateCreatorFactories
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
