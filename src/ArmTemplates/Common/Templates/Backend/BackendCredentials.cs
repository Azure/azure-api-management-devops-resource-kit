// --------------------------------------------------------------------------
//  <copyright file="BackendCredentials.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend
{
    public class BackendCredentials
    {
        public string[] Certificate { get; set; }
        public object Query { get; set; }
        public object Header { get; set; }
        public BackendCredentialsAuthorization Authorization { get; set; }
    }
}
