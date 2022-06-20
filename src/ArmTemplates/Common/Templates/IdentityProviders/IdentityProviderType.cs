// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.IdentityProviders
{
    public static class IdentityProviderType
    {
        public const string AAD = "aad";
        public const string AADB2C = "aadB2C";
        public const string Facebook = "facebook";
        public const string Google = "google";
        public const string Microsoft = "microsoft";
        public const string Twitter = "twitter";

        static HashSet<string> ClientSecretSupportedIdentityProviders = new() {
            AAD,
            AADB2C,
            Facebook,
            Google,
            Microsoft,
            Twitter
        };

        public static bool HasClientSecret(string identityProviderType) {
            return ClientSecretSupportedIdentityProviders.Contains(identityProviderType);
        }
    }
}