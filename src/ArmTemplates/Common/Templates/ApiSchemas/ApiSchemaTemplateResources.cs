// --------------------------------------------------------------------------
//  <copyright file="ApiSchemaTemplateResources.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiSchemas
{
    public class ApiSchemaTemplateResources : ITemplateResources
    {
        public List<ApiSchemaTemplateResource> ApiSchemas { get; set; } = new();

        public TemplateResource[] BuildTemplateResources()
        {
            return this.ApiSchemas.ToArray();
        }

        public bool HasContent()
        {
            return !this.ApiSchemas.IsNullOrEmpty();
        }
    }
}
