using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis
{
    public class ProductApisTemplateResource : TemplateResource
    {
        public ProductApisProperties Properties { get; set; }
    }
}