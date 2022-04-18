// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.TagApi
{
    public class TagApiTemplateResources : ITemplateResources
    {
        public List<TagTemplateResource> Tags { get; set; } = new();

        public void AddDataResources(TagApiTemplateResources otherResources)
        {
            if (otherResources is null)
            {
                return;
            }

            if (!otherResources.Tags.IsNullOrEmpty())
            {
                this.Tags.AddRange(otherResources.Tags);
            }
        }

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
