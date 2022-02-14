using Newtonsoft.Json;
using System.Collections.Generic;

namespace apimtemplate.Common.Templates.Abstractions
{
    public class Template
    {
        [JsonProperty(PropertyName = "$schema")]
        public string schema { get; set; }
        public string contentVersion { get; set; }
        public Dictionary<string, TemplateParameterProperties> parameters { get; set; }
        public object variables { get; set; }
        public TemplateResource[] resources { get; set; }
        public object outputs { get; set; }
    }
}