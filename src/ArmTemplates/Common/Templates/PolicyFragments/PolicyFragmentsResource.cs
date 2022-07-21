// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Newtonsoft.Json;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.PolicyFragments
{
    public class PolicyFragmentsResource : TemplateResource
    {
        [JsonIgnore]
        public string OriginalName { get; set; }

        public PolicyFragmentsProperties Properties { get; set; }
    }
}
