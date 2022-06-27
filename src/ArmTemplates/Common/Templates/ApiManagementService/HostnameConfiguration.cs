// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiManagementService
{
    public class HostnameConfiguration
    {
        public CertificateInformation Certificate { get; set; }

        public string CertificatePassword { get; set; }

        public string CertificateSource { get; set; }

        public string CertificateStatus { get; set; }

        public bool DefaultSslBinding { get; set; }

        public string EncodedCertificate { get; set; }

        public string HostName { get; set; }

        public string IdentityClientId { get; set; }

        public string KeyVaultId { get; set; }

        public bool NegotiateClientCertificate { get; set; }

        public string Type { get; set; }
    }
}
