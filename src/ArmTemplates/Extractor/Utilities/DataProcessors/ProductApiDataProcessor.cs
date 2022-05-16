// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors
{
    public class ProductApiDataProcessor: IProductApiDataProcessor
    {
        public IDictionary<string, string> OverrideRules { get; }
        public ProductApiDataProcessor() {
            this.OverrideRules = new Dictionary<string, string>() {
                { "Starter", "starter" },
                { "Unlimited", "unlimited" }
            };
        }

        public void ProcessData(List<ProductApiTemplateResource> productApiTemplates, ExtractorParameters extractorParameters)
        {

            if (productApiTemplates.IsNullOrEmpty())
            {
                return;
            }

            foreach (var productApiTemplate in productApiTemplates)
            {
                // save Original name for future references
                productApiTemplate.OriginalName = productApiTemplate.Name;
                productApiTemplate.NewName = productApiTemplate.Name;
                
                if (extractorParameters.OverrideProductGuids)
                {
                    this.OverrideName(productApiTemplate);
                }
            }

        }

        public void OverrideName(ProductApiTemplateResource template)
        {
            if (this.OverrideRules == null)
            {
                return;
            }

            if (this.OverrideRules.ContainsKey(template.Properties.DisplayName))
            {
                var newName = this.OverrideRules[template.Properties.DisplayName];

                if (!template.Name.Equals(newName))
                {
                    template.OriginalName = template.Name;
                    template.NewName = newName;
                    template.Name = newName;
                }
            }
        }
    }
}
