using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    public class PropertyTemplateResource : TemplateResource
    {
        public PropertyResourceProperties properties { get; set; }
    }

    public class PropertyResourceProperties
    {
        public string displayName { get; set; }
        public string value { get; set; }
        public IList<string> tags { get; set; }
        public bool secret { get; set; }
    }
}
