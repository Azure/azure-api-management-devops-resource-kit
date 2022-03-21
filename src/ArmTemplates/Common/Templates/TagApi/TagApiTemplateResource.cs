using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.TagApi
{
    public class TagApiTemplateResource : TemplateResource
    {
        public TagApiProperties Properties { get; set; }
    }
}