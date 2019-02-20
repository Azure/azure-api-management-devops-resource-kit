
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{

    public class PolicyTemplateResource : APITemplateSubResource
    {
        public PolicyTemplateProperties properties { get; set; }
    }

    public class PolicyTemplateProperties
    {
        public string policyContent { get; set; }
        public string contentFormat { get; set; }
    }
}
