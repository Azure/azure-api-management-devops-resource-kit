using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger
{
    public class LoggerTemplateResource : TemplateResource
    {
        public LoggerTemplateProperties Properties { get; set; }
    }
}
