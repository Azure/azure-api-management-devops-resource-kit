// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.IdentityProviders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors
{
    public class IdentityProviderProcessor: IIdentityProviderProcessor
    {
        public IDictionary<string, string> OverrideRules { get; }            

        public void ProcessData(List<IdentityProviderResource> identityProviderResources, ExtractorParameters extractorParameters)
        {
            if (identityProviderResources.IsNullOrEmpty())
            {
                return;
            }

            foreach (var identityProviderTemplate in identityProviderResources)
            {
                identityProviderTemplate.OriginalName = identityProviderTemplate.Name;
            }
        }
    }
}
