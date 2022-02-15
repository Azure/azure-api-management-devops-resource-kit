using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels
{
    public class PropertyTemplateResource : TemplateResource
    {
        public PropertyResourceProperties properties { get; set; }
    }

    public class PropertyResourceProperties
    {
        public IList<string> tags { get; set; }
        public bool secret { get; set; }
        public string displayName { get; set; }
        public string value { get; set; }
        public PropertyResourceKeyVaultProperties keyVault { get; set; }
    }

    public class PropertyResourceKeyVaultProperties
    {
        public string secretIdentifier { get; set; }
    }
}
