// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.AuthorizationServer
{
    public class AuthorizationServerProperties
    {
        public string Description { get; set; }
        public string[] AuthorizationMethods { get; set; }
        public string[] ClientAuthenticationMethod { get; set; }
        public AuthorizationServerTokenBodyParameter[] TokenBodyParameters { get; set; }
        public string TokenEndpoint { get; set; }
        public bool SupportState { get; set; }
        public string DefaultScope { get; set; }
        public string[] BearerTokenSendingMethods { get; set; }
        public string ClientSecret { get; set; }
        public string ResourceOwnerUsername { get; set; }
        public string ResourceOwnerPassword { get; set; }
        public string DisplayName { get; set; }
        public string ClientRegistrationEndpoint { get; set; }
        public string AuthorizationEndpoint { get; set; }
        public string[] GrantTypes { get; set; }
        public string ClientId { get; set; }
    }
}
