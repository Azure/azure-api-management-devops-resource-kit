// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Gateway
{
    public class GatewayTemplateResources : ITemplateResources
    {
        public List<GatewayTemplateResource> Gateways { get; set; } = new();

        public TemplateResource[] BuildTemplateResources()
        {
            return this.Gateways.ToArray();
        }

        public bool HasContent()
        {
            return !this.Gateways.IsNullOrEmpty();
        }
    }
}
