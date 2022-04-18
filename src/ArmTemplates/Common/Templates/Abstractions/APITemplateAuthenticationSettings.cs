// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions
{
    public class APITemplateAuthenticationSettings
    {
        public APITemplateOAuth2 OAuth2 { get; set; }

        public APITemplateOpenID Openid { get; set; }

        public bool SubscriptionKeyRequired { get; set; }
    }
}
