// --------------------------------------------------------------------------
//  <copyright file="TagTemplateResources.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags
{
    public class TagTemplateResources : ITemplateResources
    {
        public List<TagTemplateResource> Tags { get; set; } = new();

        public TemplateResource[] BuildTemplateResources()
        {
            return this.Tags.ToArray();
        }

        public bool HasContent()
        {
            return !this.Tags.IsNullOrEmpty();
        }
    }
}
