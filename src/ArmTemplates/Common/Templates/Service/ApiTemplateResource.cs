// --------------------------------------------------------------------------
//  <copyright file="ServiceApiResource.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Service
{
    public class ApiTemplateResource : TemplateResource
    {
        public ApiProperties Properties { get; set; }
    }
}
