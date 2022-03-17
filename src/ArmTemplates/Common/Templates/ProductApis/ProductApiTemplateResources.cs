// --------------------------------------------------------------------------
//  <copyright file="ProductApiTemplateResources.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis
{
    public class ProductApiTemplateResources : ITemplateResources
    {
        public List<ProductApiTemplateResource> ProductApis { get; set; } = new();

        public TemplateResource[] BuildTemplateResources()
        {
            return this.ProductApis.ToArray();
        }

        public bool HasContent()
        {
            return !this.ProductApis.IsNullOrEmpty();
        }
    }
}
