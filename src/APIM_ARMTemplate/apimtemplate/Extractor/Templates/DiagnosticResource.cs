using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class DiagnosticResource : Resource
    {
        public string type { get; set; }
        public string name { get; set; }
        public string apiVersion { get; set; }
        public object scale { get; set; }
        public DiagnosticTemplateProperties properties { get; set; }
        public string[] dependsOn { get; set; }
    }
}
