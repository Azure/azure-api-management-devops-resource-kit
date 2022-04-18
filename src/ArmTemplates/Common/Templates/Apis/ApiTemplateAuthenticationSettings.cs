// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis
{
    public class ApiTemplateAuthenticationSettings
    {
        public ApiTemplateOAuth2 OAuth2 { get; set; }

        public ApiTemplateOpenID Openid { get; set; }

        public bool SubscriptionKeyRequired { get; set; }
    }
}
