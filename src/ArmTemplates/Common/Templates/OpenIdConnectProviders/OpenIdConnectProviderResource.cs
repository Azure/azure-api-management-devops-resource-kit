// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.OpenIdConnectProviders
{
    public class OpenIdConnectProviderResource : TemplateResource
    {
        [JsonIgnore]
        public string OriginalName { get; set; }

        public OpenIdConnectProviderProperties Properties { get; set; }
    }
}
