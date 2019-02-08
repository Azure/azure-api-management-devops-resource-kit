
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class MasterTemplateCreatorFactory
    {
        public static MasterTemplateCreator GenerateMasterTemplateCreator()
        {
            TemplateCreator templateCreator = new TemplateCreator();
            MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator(templateCreator);
            return masterTemplateCreator;
        }
    }
}
