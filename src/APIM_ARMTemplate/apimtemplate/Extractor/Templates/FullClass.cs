using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class ApimServiceName
    {
        public string type { get; set; }
    }

    public class Metadata
    {
        public string description { get; set; }
    }

    public class RepoBaseUrl
    {
        public string type { get; set; }
        public Metadata metadata { get; set; }
    }

    public class Parameters
    {
        public ApimServiceName ApimServiceName { get; set; }
        public RepoBaseUrl repoBaseUrl { get; set; }
    }

    public class Variables
    {
    }

    public class Respons
    {
        public int statusCode { get; set; }
        public string description { get; set; }
        public List<object> headers { get; set; }
    }

    public class Properties
    {

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

    public class PoliciesProperties : Properties
    {  // "type": "Microsoft.ApiManagement/service/apis/policies"
        public string policyContent { get; set; }
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

    public class Resource
    { 
        
    }

    public class APIResource : Resource
    {
        public string type { get; set; }
        public string name { get; set; }
        public string apiVersion { get; set; }
        public object scale { get; set; }
        public APIProperties properties { get; set; }
        public string[] dependsOn { get; set; }
    }

    public class OperationResource : Resource
    {
        public string type { get; set; }
        public string name { get; set; }
        public string apiVersion { get; set; }
        public object scale { get; set; }
        public OperationProperties properties { get; set; }
        public string[] dependsOn { get; set; }
    }
    public class VersionSetProperties : Properties
    {
        public string description { get; set; }
        public string versionQueryName { get; set; }
        public string displayName { get; set; }
        public string versioningScheme { get; set; }
    } 
    public class VersionSetResource : Resource
    {
        public string name { get; set; }
        public string type { get; set; }
        public string apiVersion { get; set; }
        public VersionSetProperties properties { get; set; }
    }

    public class ApiPoliciesResource : Resource
    {
        public string type { get; set; }
        public string name { get; set; }
        public string apiVersion { get; set; }
        public PoliciesProperties properties { get; set; }
        public string[] dependsOn { get; set; }
    }

    public class ExtractorTemplateParameterProperties
    {
        public string type { get; set; }
        public TemplateParameterMetadata metadata { get; set; }
        public string[] allowedValues { get; set; }
        public string defaultValue { get; set; }
        public string value { get; set; }
    }

    public class TemplateParameterMetadata
    {
        public string description { get; set; }
    }

    public class ARMTemplate
    {
        [JsonProperty(PropertyName = "$schema")]
        public string schema { get; set; }
        public string contentVersion { get; set; }
        public Dictionary<string, ExtractorTemplateParameterProperties> parameters { get; set; }
        public Variables variables { get; set; }
        public List<Resource> resources { get; set; }
        public object outputs { get; internal set; }
    }
}