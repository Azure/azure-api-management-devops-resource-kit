// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------


namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiManagementService
{
    public class CertificateConfiguration
    {
        public CertificateInformation Certificate { get; set; }

        public string CertificatePassword { get; set; }

        public string EncodedCertificate { get; set; }

        public string StoreName { get; set; }
    }
}
