
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    public class TagDescriptionTemplateResource : TemplateResource
    {
        public TagDescriptionTemplateProperties properties { get; set; }
    }

    public class TagDescriptionTemplateProperties
    {
        public string description { get; set; }
        public string externalDocsUrl { get; set; }
        public string externalDocsDescription { get; set; }
    }
}
