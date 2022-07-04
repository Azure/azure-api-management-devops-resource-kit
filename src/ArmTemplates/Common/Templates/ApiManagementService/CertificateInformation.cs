// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiManagementService
{
    public class CertificateInformation
    {
        public string Expiry { get; set; }

        public string Subject { get; set; }

        public string Thumbprint { get; set; }
    }
}
