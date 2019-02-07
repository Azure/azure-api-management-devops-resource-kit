
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
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
            APITemplateCreator apiTemplateCreator = new APITemplateCreator(fileReader, templateCreator, policyTemplateCreator, productAPITemplateCreator, diagnosticTemplateCreator);
            return apiTemplateCreator;
        }
    }
}
