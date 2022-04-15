// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Groups
{
    public class GroupTemplateResources : ITemplateResources
    {
        public List<GroupTemplateResource> Groups { get; set; } = new();

        public TemplateResource[] BuildTemplateResources()
        {
            return this.Groups.ToArray();
        }

        public bool HasContent()
        {
            return !this.Groups.IsNullOrEmpty();
        }
    }
}
