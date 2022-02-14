using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels
{
    public class TagAPITemplateResource : TemplateResource
    {
        public TagAPITemplateProperties properties { get; set; }
    }

    public class TagAPITemplateProperties
    {
        public string displayName { get; set; }
    }
}