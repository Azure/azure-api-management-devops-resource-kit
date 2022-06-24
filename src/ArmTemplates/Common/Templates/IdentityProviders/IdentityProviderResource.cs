// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.IdentityProviders
{
    public class IdentityProviderResource : TemplateResource
    {
        [JsonIgnore]
        public string OriginalName { get; set; }

        public IdentityProviderProperties Properties { get; set; }
    }
}
