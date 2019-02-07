using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class ArmTemplate
    {
        [JsonProperty(PropertyName = "$schema")]
        public string schema { get; set; }
        public string contentVersion { get; set; }
        public Dictionary<string, ExtractorTemplateParameterProperties> parameters { get; set; }
        public Variables variables { get; set; }
        public List<Resource> resources { get; set; }
        public object outputs { get; internal set; }
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

    public class Variables
    {
    }
    public class Resource
    {
    }
    public class Properties
    {
    }
}
