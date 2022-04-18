// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.GatewayApi
{
    public class GatewayApiTemplateResources : ITemplateResources
    {
        public List<GatewayApiTemplateResource> GatewayApis { get; set; } = new();

        public TemplateResource[] BuildTemplateResources()
        {
            return this.GatewayApis.ToArray();
        }

        public bool HasContent()
        {
            return !this.GatewayApis.IsNullOrEmpty();
        }
    }
}
