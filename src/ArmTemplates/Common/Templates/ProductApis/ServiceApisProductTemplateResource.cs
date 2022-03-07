using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis
{
    public class ServiceApisProductTemplateResource : TemplateResource
    {
        public ServiceApiProductProperties Properties { get; set; }
    }
}