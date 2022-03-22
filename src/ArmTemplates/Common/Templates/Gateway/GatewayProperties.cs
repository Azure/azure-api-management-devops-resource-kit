// --------------------------------------------------------------------------
//  <copyright file="GatewayProperties.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Gateway
{
    public class GatewayProperties
    {
        public string Description { get; set; }

        public GatewayLocationData LocationData { get; set; }
    }
}
