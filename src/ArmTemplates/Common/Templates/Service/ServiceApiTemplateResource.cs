// --------------------------------------------------------------------------
//  <copyright file="ServiceApiResource.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Service
{
    public class ServiceApiTemplateResource : TemplateResource
    {
        public ServiceApiProperties Properties { get; set; }
    }
}
