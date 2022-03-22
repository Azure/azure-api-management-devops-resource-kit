// --------------------------------------------------------------------------
//  <copyright file="GatewayApiTemplateResource.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.GatewayApi
{
    public class GatewayApiTemplateResource : TemplateResource
    {
        public GatewayApiProperties Properties { get; set; }
    }
}
