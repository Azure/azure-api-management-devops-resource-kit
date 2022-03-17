// --------------------------------------------------------------------------
//  <copyright file="ApiSchemaTemplateResource.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiSchemas
{
    public class ApiSchemaTemplateResource : TemplateResource
    {
        public ApiSchemaProperties Properties { get; set; }
    }
}
