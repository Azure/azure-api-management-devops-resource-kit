// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiManagementService
{
    public class ApiManagementServiceIdentity
    {
        public string PrincipalId { get; set; }

        public string TenantId { get; set; }

        public string Type { get; set; }

        public IDictionary<string, UserIdentityProperties> UserAssignedIdentities { get; set; }
    }
}
