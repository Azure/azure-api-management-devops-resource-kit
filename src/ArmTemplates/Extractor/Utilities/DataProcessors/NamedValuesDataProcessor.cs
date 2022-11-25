// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors
{
    public class NamedValuesDataProcessor : INamedValuesDataProcessor
    {
        public void ProcessData(List<NamedValueTemplateResource> templateResources)
        {
            if (templateResources.IsNullOrEmpty())
            {
                return;
            }

            foreach (var templateResource in templateResources)
            {
                this.ProcessSingleData(templateResource);
            }
        }

        public void ProcessSingleData(NamedValueTemplateResource templateResource)
        {
            if (templateResource == null)
            {
                return;
            }
            
            templateResource.OriginalName = templateResource.Name;
            templateResource.Properties.OriginalValue = templateResource.Properties?.Value;

            templateResource.Properties.OriginalKeyVaultSecretIdentifierValue = templateResource.Properties.KeyVault?.SecretIdentifier;
        }
    }
}
