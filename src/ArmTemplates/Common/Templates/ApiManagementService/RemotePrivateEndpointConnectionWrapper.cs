// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiManagementService
{
    public class RemotePrivateEndpointConnectionWrapper
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public RemotePrivateEndpointConnectionWrapperProperties Properties { get; set; }
    }
}
