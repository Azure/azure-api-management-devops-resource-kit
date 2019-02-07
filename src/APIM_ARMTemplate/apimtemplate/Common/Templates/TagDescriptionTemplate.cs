using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class TagDescriptionTemplate
    {
        public string name { get; set; }
        public string type { get; set; }
        public string apiVersion { get; set; }
        public TagDescriptionTemplateProperties properties { get; set; }
    }

    public class TagDescriptionTemplateProperties
    {
        public string description { get; set; }
        public string externalDocsUrl { get; set; }
        public string externalDocsDescription { get; set; }
    }
}
