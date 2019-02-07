using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class ApiPoliciesResource : Resource
    {
        public string type { get; set; }
        public string name { get; set; }
        public string apiVersion { get; set; }
        public PoliciesProperties properties { get; set; }
        public string[] dependsOn { get; set; }
    }
    public class PoliciesProperties : Properties
    {  // "type": "Microsoft.ApiManagement/service/apis/policies"
        public string policyContent { get; set; }
    }
}