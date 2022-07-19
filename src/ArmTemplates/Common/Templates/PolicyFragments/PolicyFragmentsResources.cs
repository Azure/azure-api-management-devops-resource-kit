// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.PolicyFragments
{
    public class PolicyFragmentsResources : ITemplateResources
    {
        public List<PolicyFragmentsResource> PolicyFragments { get; set; } = new();

        public TemplateResource[] BuildTemplateResources()
        {
            return this.PolicyFragments.ToArray();
        }

        public bool HasContent()
        {
            return !this.PolicyFragments.IsNullOrEmpty();
        }
    }
}
