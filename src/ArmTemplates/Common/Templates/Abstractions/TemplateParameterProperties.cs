using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Applications;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions
{
    public class TemplateParameterProperties
    {
        public string Type { get; set; }
        public TemplateParameterMetadata Metadata { get; set; }
        public string[] AllowedValues { get; set; }
        public string DefaultValue { get; set; }
        public string Value { get; set; }

        public static TemplateParameterProperties Create(string metadataDescription, string type) => new()
        {
            Metadata = new()
            {
                Description = metadataDescription,
            },
            Type = type
        };    
    }
}
