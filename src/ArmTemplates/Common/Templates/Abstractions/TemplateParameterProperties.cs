namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions
{
    public class TemplateParameterProperties
    {
        public string type { get; set; }
        public TemplateParameterMetadata metadata { get; set; }
        public string[] allowedValues { get; set; }
        public string defaultValue { get; set; }
        public string value { get; set; }
    }
}
