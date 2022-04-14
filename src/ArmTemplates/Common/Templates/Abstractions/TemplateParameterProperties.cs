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

        public TemplateParameterProperties()
        {
        }

        public TemplateParameterProperties(string metadataDescription, string type)
        {
            this.Metadata = new()
            {
                Description = metadataDescription,
            };
            this.Type = type;
        }  
    }
}
