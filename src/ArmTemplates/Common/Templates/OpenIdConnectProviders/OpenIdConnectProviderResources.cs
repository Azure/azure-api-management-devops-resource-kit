// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.OpenIdConnectProviders
{
    public class OpenIdConnectProviderResources: TemplateResourcesBase, ITemplateResources
    {
        public List<OpenIdConnectProviderResource> OpenIdConnectProviders { get; set; } = new();

        public TemplateResource[] BuildTemplateResources()
        {
            return this.OpenIdConnectProviders.ToArray();
        }

        public bool HasContent()
        {
            return !this.OpenIdConnectProviders.IsNullOrEmpty();
        }
    }
}
