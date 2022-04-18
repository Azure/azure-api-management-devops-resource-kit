// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Groups;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products
{
    public class ProductTemplateResources : TemplateResourcesBase, ITemplateResources
    {
        public List<ProductsTemplateResource> Products { get; set; } = new();

        public List<TagTemplateResource> Tags { get; set; } = new();

        public List<GroupTemplateResource> Groups { get; set; } = new();

        public List<PolicyTemplateResource> Policies { get; set; } = new();

        public TemplateResource[] BuildTemplateResources()
        {
            return this.ConcatenateTemplateResourcesCollections(
                this.Products.ToArray(),
                this.Tags.ToArray(),
                this.Groups.ToArray(),
                this.Policies.ToArray());
        }

        public bool HasContent()
        {
            return !this.Products.IsNullOrEmpty() 
                || !this.Tags.IsNullOrEmpty()
                || !this.Groups.IsNullOrEmpty()
                || !this.Policies.IsNullOrEmpty();
        }
    }
}
