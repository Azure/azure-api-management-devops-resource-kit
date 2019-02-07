
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class APIVersionSetTemplateCreatorFactory
    {
        public static APIVersionSetTemplateCreator GenerateAPIVersionSetTemplateCreator()
        {
            TemplateCreator templateCreator = new TemplateCreator();
            APIVersionSetTemplateCreator apiVersionSetTemplateCreator = new APIVersionSetTemplateCreator(templateCreator);
            return apiVersionSetTemplateCreator;
        }
    }
}
