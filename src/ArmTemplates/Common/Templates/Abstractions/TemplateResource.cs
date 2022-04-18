// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions
{
    public class TemplateResource
    {
        [JsonProperty(Order = -10)]
        public string ApiVersion { get; set; }

        [JsonProperty(Order = -8)]
        public string Type { get; set; }

        [JsonProperty(Order = -6)]
        public string Name { get; set; }

        [JsonProperty(Order = -4)]
        public string Scale { get; set; }

        [JsonProperty(Order = -2)]
        public string[] DependsOn { get; set; }
    }
}
