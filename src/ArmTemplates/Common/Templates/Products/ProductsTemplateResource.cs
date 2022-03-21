// --------------------------------------------------------------------------
//  <copyright file="ProductsTemplateResource.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products
{
    public class ProductsTemplateResource : TemplateResource
    {
        public ProductsProperties Properties { get; set; }
    }
}
