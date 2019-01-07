using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class APIVersionSetTemplateResource : TemplateResource
    {
        public APIVersionSetProperties properties { get; set; }
    }

    public class APIVersionSetProperties
    {
        public string displayName { get; set; }
        public string description { get; set; }
        public string versionQueryName { get; set; }
        public string versionHeaderName { get; set; }
        public string versioningScheme { get; set; }
    }
}
