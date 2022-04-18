// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues
{
    public class NamedValueProperties
    {
        public IList<string> Tags { get; set; }
        public bool Secret { get; set; }
        public string DisplayName { get; set; }
        public string Value { get; set; }
        public NamedValueResourceKeyVaultProperties KeyVault { get; set; }
    }
}
