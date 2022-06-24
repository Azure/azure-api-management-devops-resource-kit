// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.IdentityProviders
{
    public class IdentityProviderResources : TemplateResourcesBase, ITemplateResources
    {
        public List<IdentityProviderResource> IdentityProviders { get; set; } = new();

        public TemplateResource[] BuildTemplateResources()
        {
            return this.IdentityProviders.ToArray();
        }

        public bool HasContent()
        {
            return !this.IdentityProviders.IsNullOrEmpty();
        }
    }
}
