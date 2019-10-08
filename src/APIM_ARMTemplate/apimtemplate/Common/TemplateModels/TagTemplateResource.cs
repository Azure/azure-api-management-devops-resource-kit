
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{

    public class TagTemplateResource : APITemplateSubResource
    {
        public TagTemplateProperties properties { get; set; }
    }

    public class TagTemplateProperties
    { 
        public string displayName {get; set;}
    }
}
