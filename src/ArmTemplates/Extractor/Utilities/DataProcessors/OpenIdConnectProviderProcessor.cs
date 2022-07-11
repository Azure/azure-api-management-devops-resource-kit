// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.OpenIdConnectProviders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors
{
    public class OpenIdConnectProviderProcessor : IOpenIdConnectProviderProcessor
    {
        public IDictionary<string, string> OverrideRules { get; }            

        public void ProcessData(List<OpenIdConnectProviderResource> openIdConnectProviderResources, ExtractorParameters extractorParameters)
        {
            if (openIdConnectProviderResources.IsNullOrEmpty())
            {
                return;
            }

            foreach (var openIdConnectProviderTemplate in openIdConnectProviderResources)
            {
                openIdConnectProviderTemplate.OriginalName = openIdConnectProviderTemplate.Name;
            }
        }
    }
}
