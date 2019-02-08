using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class OperationResource : Resource
    {
        public string type { get; set; }
        public string name { get; set; }
        public string apiVersion { get; set; }
        public object scale { get; set; }
        public OperationProperties properties { get; set; }
        public string[] dependsOn { get; set; }
    }
    public class OperationProperties : Properties
    {  // "type": "Microsoft.ApiManagement/service/apis/operations"
        public string displayName { get; set; }
        public string method { get; set; }
        public string urlTemplate { get; set; }
        public List<object> templateParameters { get; set; }
        public List<Respons> responses { get; set; }
        public object policies { get; set; }
    }
    public class Respons
    {
        public int statusCode { get; set; }
        public string description { get; set; }
        public List<object> headers { get; set; }
    }
}
