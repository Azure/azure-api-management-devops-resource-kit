// --------------------------------------------------------------------------
//  <copyright file="GatewayTemplateResource.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Gateway
{
    public class GatewayTemplateResource : TemplateResource
    {
        public GatewayProperties Properties { get; set; }
    }
}
