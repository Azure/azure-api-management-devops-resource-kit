// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions
{
    public class APITemplateOpenID
    {
        public string OpenIdProviderId { get; set; }

        public string[] BearerTokenSendingMethods { get; set; }
    }
}
