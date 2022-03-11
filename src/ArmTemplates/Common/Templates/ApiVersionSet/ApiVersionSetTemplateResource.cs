// --------------------------------------------------------------------------
//  <copyright file="ApiVersionSetTemplateResource.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiVersionSet
{
    public class ApiVersionSetTemplateResource : TemplateResource
    {
        public ApiVersionSetProperties Properties { get; set; }
    }
}
