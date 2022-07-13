// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------


namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.OpenIdConnectProviders
{
    public class OpenIdConnectProviderProperties
    {
        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string MetadataEndpoint { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }
    }
}
