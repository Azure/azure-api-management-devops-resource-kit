using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis
{
    public class ProductApiTemplateResource : TemplateResource
    {
        public ProductApiProperties Properties { get; set; }
    }
}