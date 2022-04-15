// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy
{
    public class PolicyTemplateProperties
    {
        [JsonIgnore]
        public string PolicyContentFileFullPath { get; set; }

        [JsonProperty("value")]
        public string PolicyContent { get; set; }

        public string Format { get; set; }
    }
}
