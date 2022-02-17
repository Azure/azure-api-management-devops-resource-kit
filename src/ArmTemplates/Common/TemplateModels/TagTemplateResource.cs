using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels
{

    public class TagTemplateResource : APITemplateSubResource
    {
        public TagTemplateProperties Properties { get; set; }
    }

    public class TagTemplateProperties
    {
        public string displayName { get; set; }
    }
}
