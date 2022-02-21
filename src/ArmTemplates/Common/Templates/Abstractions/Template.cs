using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions
{
    public class Template
    {
        [JsonProperty(PropertyName = "$schema")]
        public string Schema { get; set; }

        public string ContentVersion { get; set; }

        public Dictionary<string, TemplateParameterProperties> Parameters { get; set; }

        public object Variables { get; set; }

        public TemplateResource[] Resources { get; set; }

        public object Outputs { get; set; }

        /// <summary>
        /// Returns true, if template contains any resource
        /// </summary>
        [JsonIgnore]
        public bool HasResources => !this.Resources.IsNullOrEmpty();
    }
}