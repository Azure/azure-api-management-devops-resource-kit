
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class PropertyResource : Resource
    {
        public string type { get; set; }
        public string name { get; set; }
        public string apiVersion { get; set; }
        public object scale { get; set; }
        public PropertyResourceProperties properties { get; set; }
        public string[] dependsOn { get; set; }
    }

    public class PropertyResourceProperties
    {
        public string displayName { get; set; }
        public string value { get; set; }
        public IList<string> tags { get; set; }
        public bool secret { get; set; }
    }
}
