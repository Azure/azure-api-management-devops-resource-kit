// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers
{
    public class FileNames
    {
        public string ApiVersionSets { get; set; }

        public string AuthorizationServers { get; set; }

        public string Backends { get; set; }

        public string GlobalServicePolicy { get; set; }

        public string Loggers { get; set; }

        public string NamedValues { get; set; }

        public string Tags { get; set; }

        public string Products { get; set; }

        public string ProductAPIs { get; set; }

        public string Groups { get; set; }

        public string TagApi { get; set; }

        public string Gateway { get; set; }

        public string GatewayApi { get; set; }

        public string Parameters { get; set; }
        
        // linked property outputs 1 master template
        public string LinkedMaster { get; set; }

        public string Apis { get; set; }

        public string SplitAPIs { get; set; }

        public string VersionSetMasterFolder { get; set; }

        public string RevisionMasterFolder { get; set; }

        public string GroupAPIsMasterFolder { get; set; }

        public string BaseFileName { get; set; }
    }
}
