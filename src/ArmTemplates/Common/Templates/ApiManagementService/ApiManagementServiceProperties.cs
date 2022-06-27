// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiManagementService
{
    public class ApiManagementServiceProperties
    {
        public AdditionalLocation[] AdditionalLocations { get; set; }

        public ApiVersionConstraint ApiVersionConstraint { get; set; }

        public CertificateConfiguration[] Certificates { get; set; }

        public IDictionary<string, string> CustomProperties { get; set; }

        public bool DisableGateway { get; set; }

        public bool? EnableClientCertificate { get; set; }
        
        //TODO check possibility to avoid error with Proxy type
        //public HostnameConfiguration[] HostnameConfigurations { get; set; }

        public string NotificationSenderEmail { get; set; }

        public string PlatformVersion { get; set; }

        public RemotePrivateEndpointConnectionWrapper[] PrivateEndpointConnections { get; set; }

        public string[] PrivateIPAddresses { get; set; }

        public string ProvisioningState { get; set; }

        public string PublicIpAddressId { get; set; }

        public string PublicNetworkAccess { get; set; }

        public string PublisherEmail { get; set; }

        public string PublisherName { get; set; }

        public bool Restore { get; set; }

        public string TargetProvisioningState { get; set; }

        public VirtualNetworkConfiguration VirtualNetworkConfiguration { get; set; }

        public string VirtualNetworkType { get; set; }
    }
}
