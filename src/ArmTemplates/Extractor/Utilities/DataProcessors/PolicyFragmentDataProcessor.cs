// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.PolicyFragments;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors
{
    public class PolicyFragmentDataProcessor: IPolicyFragmentDataProcessor
    {
        public void ProcessData(List<PolicyFragmentsResource> policyFragmentResources, ExtractorParameters extractorParameters)
        {
            if (policyFragmentResources.IsNullOrEmpty())
            {
                return;
            }

            foreach (var policyFragment in policyFragmentResources)
            {
                policyFragment.OriginalName = policyFragment.Name;
            }
        }
    }
}
