// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models
{
    public class ApiOauth2ScopeProperty
    {
        public string ApiName { get; private set; }

        public string Scope { get; private set; }

        public ApiOauth2ScopeProperty(string apiName, string scope)
        {
            this.ApiName = apiName;
            this.Scope = scope;
        }
    }
}
