using apimtemplate.Common.FileHandlers;
using apimtemplate.Creator.TemplateCreators;

namespace apimtemplate.test.Creator.TemplateCreatorFactories
{
    public class APITemplateCreatorFactory
    {
        public static APITemplateCreator GenerateAPITemplateCreator()
        {
            FileReader fileReader = new FileReader();
            TemplateCreator templateCreator = new TemplateCreator();
            PolicyTemplateCreator policyTemplateCreator = new PolicyTemplateCreator(fileReader);
            ProductAPITemplateCreator productAPITemplateCreator = new ProductAPITemplateCreator();
            DiagnosticTemplateCreator diagnosticTemplateCreator = new DiagnosticTemplateCreator();
            ReleaseTemplateCreator releaseTemplateCreator = new ReleaseTemplateCreator();
            TagAPITemplateCreator tagAPITemplateCreator = new TagAPITemplateCreator();
            APITemplateCreator apiTemplateCreator = new APITemplateCreator(fileReader, policyTemplateCreator, productAPITemplateCreator, tagAPITemplateCreator, diagnosticTemplateCreator, releaseTemplateCreator);
            return apiTemplateCreator;
        }
    }
}
