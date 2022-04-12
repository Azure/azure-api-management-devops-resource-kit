namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions
{
    public class TemplateParameterProperties
    {
        public string Type { get; set; }
        public TemplateParameterMetadata Metadata { get; set; }
        public string[] AllowedValues { get; set; }
        public string DefaultValue { get; set; }
        public string Value { get; set; }
    }
}
