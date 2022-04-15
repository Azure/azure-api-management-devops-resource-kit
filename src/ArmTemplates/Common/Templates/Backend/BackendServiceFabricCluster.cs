// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend
{
    public class BackendServiceFabricCluster
    {
        public string ClientCertificatethumbprint { get; set; }
        public int MaxPartitionResolutionRetries { get; set; }
        public string[] ManagementEndpoints { get; set; }
        public string[] ServerCertificateThumbprints { get; set; }
        public ServerX509Names[] ServerX509Names { get; set; }
    }
}
