// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiVersionSet
{
    public class ApiVersionSetTemplateResources : ITemplateResources
    {
        public List<ApiVersionSetTemplateResource> ApiVersionSets { get; set; } = new();

        public TemplateResource[] BuildTemplateResources()
        {
            return this.ApiVersionSets.ToArray();
        }

        public bool HasContent()
        {
            return !this.ApiVersionSets.IsNullOrEmpty();
        }
    }
}
