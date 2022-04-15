// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend
{
    public class BackendTemplateProperties
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ResourceId { get; set; }
        public BackendServiceFabricProperties Properties { get; set; }
        public BackendCredentials Credentials { get; set; }
        public BackendProxy Proxy { get; set; }
        public BackendTls Tls { get; set; }
        public string Url { get; set; }
        public string Protocol { get; set; }
    }
}
