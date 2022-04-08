using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend
{
    public class BackendTemplateResource : TemplateResource
    {
        public BackendTemplateProperties Properties { get; set; }
    }
}
