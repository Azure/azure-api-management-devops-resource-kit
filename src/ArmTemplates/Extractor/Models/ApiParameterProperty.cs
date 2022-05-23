// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models
{
    public class ApiParameterProperty
    {
        public string Oauth2Scope { get; private set; }

        public string ServiceUrl { get; private set; }

        public ApiParameterProperty(string oauth2Scope, string serviceUrl)
        {
            this.Oauth2Scope = oauth2Scope;
            this.ServiceUrl = serviceUrl;
        }
    }
}
