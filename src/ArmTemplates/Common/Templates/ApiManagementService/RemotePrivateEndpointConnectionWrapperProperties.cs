// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiManagementService
{
    public class RemotePrivateEndpointConnectionWrapperProperties
    {
        public string[] GroupIds { get; set; }

        public ArmIdWrapper PrivateEndpoint { get; set; }

        public PrivateLinkServiceConnectionState PrivateLinkServiceConnectionState { get; set; }

        public string ProvisioningState { get; set; }
    }
}
