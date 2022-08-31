// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.OpenIdConnectProviders
{
    public class OpenIdConnectProviderResource : TemplateResource
    {
        public OpenIdConnectProviderProperties Properties { get; set; }
    }
}
