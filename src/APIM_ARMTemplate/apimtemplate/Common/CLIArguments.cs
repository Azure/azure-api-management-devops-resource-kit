using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class CLICreatorArguments
    {
        public string openAPISpecFile { get; set; }
        public string openAPISpecURL { get; set; }
        public string xmlPolicyFile { get; set; }
        public string xmlPolicyURL { get; set; }
        public string outputLocation { get; set; }
        public bool linked { get; set; }
        public string path { get; set; }
        public string apiVersion { get; set; }
        public string apiRevision { get; set; }
        public string apiVersionSetId { get; set; }
        public List<string> productIds { get; set; }
    }
}
