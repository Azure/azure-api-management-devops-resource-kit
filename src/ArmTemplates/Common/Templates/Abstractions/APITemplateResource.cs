namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions
{
    public class APITemplateResource : TemplateResource
    {
        public APITemplateProperties Properties { get; set; }

        public APITemplateSubResource[] Resources { get; set; }
    }   
}