// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiManagementService
{
    public class AdditionalLocation
    {
        public string DisableGateway { get; set; }

        public string GatewayRegionalUrl { get; set; }

        public string Location { get; set; }

        public string PlatformVersion { get; set; }

        public string[] PrivateIPAddresses { get; set; }

        public string[] PublicIPAddresses { get; set; }

        public string PublicIpAddressId { get; set; }

        public ApiManagementServiceSkuProperties Sku { get; set; }

        public VirtualNetworkConfiguration VirtualNetworkConfiguration { get; set; }

        public string[] Zones { get; set; }
    }
}
