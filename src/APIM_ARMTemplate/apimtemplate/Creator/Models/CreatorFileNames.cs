using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class CreatorFileNames
    {
        public string apiVersionSet { get; set; }
        public string initialAPI { get; set; }
        public string subsequentAPI { get; set; }
        public string apiPolicy { get; set; }
        public Dictionary<string, string> operationPolicies { get; set; }
        public Dictionary<string, string> productAPIs { get; set; }
        public string master { get; set; }
    }
}
