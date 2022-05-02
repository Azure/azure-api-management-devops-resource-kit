// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions
{
    public class TemplateParameterProperties<TParameter> : TemplateParameterProperties
    {   
        public new TParameter[] AllowedValues { get; set; }

        [JsonProperty("defaultValue")]
        public new TParameter Value { get; set; }
    }
}
