// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.IdentityProviders
{
    public class IdentityProviderTemplateProperties
    {
        public string AllowedTenants { get; set; }
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string PasswordResetPolicyName { get; set; }
        public string ProfileEditingPolicyName { get; set; }
        public string SigninPolicyName { get; set; }
        public string SigninTenant { get; set; }
        public string SignupPolicyName { get; set; }
        public string Type { get; set; }
    }
}
