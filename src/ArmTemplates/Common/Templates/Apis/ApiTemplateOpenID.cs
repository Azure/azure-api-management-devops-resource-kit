// --------------------------------------------------------------------------
//  <copyright file="ApiTemplateOpenID.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis
{
    public class ApiTemplateOpenID
    {
        public string OpenIdProviderId { get; set; }

        public string[] BearerTokenSendingMethods { get; set; }
    }
}
