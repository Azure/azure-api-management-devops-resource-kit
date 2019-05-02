using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
{
    public class MasterTemplateCreatorFactory
    {
        public static MasterTemplateCreator GenerateMasterTemplateCreator()
        {
            TemplateCreator templateCreator = new TemplateCreator();
            MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator();
            return masterTemplateCreator;
        }
    }
}
