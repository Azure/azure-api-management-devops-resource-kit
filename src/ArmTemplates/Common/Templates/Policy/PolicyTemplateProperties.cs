// --------------------------------------------------------------------------
//  <copyright file="PolicyTemplateProperties.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy
{
    public class PolicyTemplateProperties
    {
        [JsonProperty("value")]
        public string PolicyContent { get; set; }

        public string Format { get; set; }
    }
}
