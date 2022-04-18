// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.AuthorizationServer
{
    public class AuthorizationServerTemplateResources : ITemplateResources
    {
        public List<AuthorizationServerTemplateResource> AuthorizationServers { get; set; } = new();

        public TemplateResource[] BuildTemplateResources()
        {
            return this.AuthorizationServers.ToArray();
        }

        public bool HasContent()
        {
            return !this.AuthorizationServers.IsNullOrEmpty();
        }
    }
}
