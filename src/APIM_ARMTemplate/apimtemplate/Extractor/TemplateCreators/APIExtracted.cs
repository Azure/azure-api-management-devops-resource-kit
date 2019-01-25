using System.Collections.Generic;
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class Properties
    {
        public string displayName { get; set; }
        public string apiRevision { get; set; }
        public string description { get; set; }
        public string serviceUrl { get; set; }
        public string path { get; set; }
        public List<string> protocols { get; set; }
        public object authenticationSettings { get; set; }
        public object subscriptionKeyParameterNames { get; set; }
        public bool isCurrent { get; set; }
        public string apiVersion { get; set; }
        public string apiVersionSetId { get; set; }
        public string apiRevisionDescription { get; set; }
        public string apiVersionDescription { get; set; }
    }

    public class Value
    {
        public string id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public Properties properties { get; set; }
    }

    public class ExtractedAPI
    {
        public List<Value> value { get; set; }
    }
}