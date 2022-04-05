// --------------------------------------------------------------------------
//  <copyright file="BackendTLS.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend
{
    public class BackendTls
    {
        public bool ValidateCertificateChain { get; set; }
        public bool ValidateCertificateName { get; set; }
    }
}
