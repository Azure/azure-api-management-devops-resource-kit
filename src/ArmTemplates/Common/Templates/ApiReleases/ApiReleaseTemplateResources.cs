// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiReleases
{
    public class ApiReleaseTemplateResources : ITemplateResources
    {
        public List<ApiReleaseTemplateResource> ApiReleases { get; set; } = new();

        public TemplateResource[] BuildTemplateResources()
        {
            return this.ApiReleases.ToArray();
        }

        public bool HasContent()
        {
            return !this.ApiReleases.IsNullOrEmpty();
        }
    }
}
