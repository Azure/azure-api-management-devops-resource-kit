// --------------------------------------------------------------------------
//  <copyright file="APITemplateOAuth2.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions
{
    public class APITemplateOAuth2
    {
        public string AuthorizationServerId { get; set; }

        public string Scope { get; set; }
    }
}
