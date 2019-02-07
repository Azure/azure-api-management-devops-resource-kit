using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class ApiResource : Resource
    {
        public string type { get; set; }
        public string name { get; set; }
        public string apiVersion { get; set; }
        public object scale { get; set; }
        public APIProperties properties { get; set; }
        public string[] dependsOn { get; set; }
    }
    public class APIProperties : Properties
    { //"type": "Microsoft.ApiManagement/service/apis"
        public string displayName { get; set; }
        public string apiRevision { get; set; }
        public string description { get; set; }
        public string serviceUrl { get; set; }
        public string path { get; set; }
        public List<string> protocols { get; set; }
        public object authenticationSettings { get; set; }
        public object subscriptionKeyParameterNames { get; set; }
        public string apiVersion { get; set; }
        public string apiVersionSetId { get; set; }
    }

}
