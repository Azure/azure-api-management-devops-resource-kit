// --------------------------------------------------------------------------
//  <copyright file="DiagnosticsTemplateResource.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Diagnostics
{
    public class DiagnosticsTemplateResource : TemplateResource
    {
        public DiagnosticsProperties Properties { get; set; }
    }
}
