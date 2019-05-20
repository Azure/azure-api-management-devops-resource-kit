namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    public class ReleaseTemplateResource : APITemplateSubResource
    {
        public ReleaseTemplateProperties properties { get; set; }
    }

    public class ReleaseTemplateProperties
    {
        public string apiId { get; set; }
        public string notes { get; set; }
    }
}