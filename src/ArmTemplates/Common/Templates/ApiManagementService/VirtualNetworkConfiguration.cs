// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiManagementService
{
    public class VirtualNetworkConfiguration
    {
        public string SubnetResourceId { get; set; }

        public string Subnetname { get; set; }

        public string Vnetid { get; set; }
    }
}
