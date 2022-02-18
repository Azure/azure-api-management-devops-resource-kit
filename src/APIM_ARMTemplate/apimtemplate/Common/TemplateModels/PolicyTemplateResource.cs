
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{

    public class PolicyTemplateResource : APITemplateSubResource
    {
        public PolicyTemplateProperties properties { get; set; }
    }

    public class PolicyTemplateProperties
    {
        public string value { get; set; }
        public string format { get; set; }
    }
}
