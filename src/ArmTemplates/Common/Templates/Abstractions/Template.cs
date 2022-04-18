// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions
{
    public class Template
    {
        [JsonProperty(PropertyName = "$schema", Order = -10)]
        public string Schema { get; set; }

        [JsonProperty(Order = -8)]
        public string ContentVersion { get; set; }

        public object Variables { get; set; }

        public object Outputs { get; set; }

        [JsonProperty(Order = 8)]
        public Dictionary<string, TemplateParameterProperties> Parameters { get; set; }

        [JsonProperty(Order = 10)]
        public virtual TemplateResource[] Resources { get; set; }

        /// <summary>
        /// Returns true, if template contains any resource
        /// </summary>
        [JsonIgnore]
        public bool HasResources => !this.Resources.IsNullOrEmpty();
    }
}