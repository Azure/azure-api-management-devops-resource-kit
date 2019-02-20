
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{

    public class ReleaseTemplateResource : APITemplateSubResource
    {
        public ReleasTemplateProperties properties { get; set; }
    }

    public class ReleasTemplateProperties
    {
        public string apiId { get; set; }
        public string notes { get; set; }
    }
}
