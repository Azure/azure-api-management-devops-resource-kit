using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy
{
    public class PolicyTemplateResource : APITemplateSubResource
    {
        public PolicyTemplateProperties Properties { get; set; }
    }
}
