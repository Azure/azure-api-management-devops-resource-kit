using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class VersionSetResource : Resource
    {
        public string name { get; set; }
        public string type { get; set; }
        public string apiVersion { get; set; }
        public VersionSetProperties properties { get; set; }
    }
    public class VersionSetProperties : Properties
    {
        public string description { get; set; }
        public string versionQueryName { get; set; }
        public string displayName { get; set; }
        public string versioningScheme { get; set; }
    }
}
