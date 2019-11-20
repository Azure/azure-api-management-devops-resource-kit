namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
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