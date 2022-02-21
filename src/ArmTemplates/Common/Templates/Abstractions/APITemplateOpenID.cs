// --------------------------------------------------------------------------
//  <copyright file="APITemplateOpenID.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions
{
    public class APITemplateOpenID
    {
        public string OpenIdProviderId { get; set; }

        public string[] BearerTokenSendingMethods { get; set; }
    }
}
