// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

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
            this.Metadata = new TemplateParameterMetadata()
            {
                Description = metadataDescription,
            };
            this.Type = type;
        }  
    }
}
