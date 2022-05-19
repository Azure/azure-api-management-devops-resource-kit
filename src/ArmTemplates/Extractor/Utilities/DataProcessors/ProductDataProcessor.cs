// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors
{
    public class ProductDataProcessor: IProductDataProcessor
    {
        public IDictionary<string, string> OverrideRules { get; }            

        public ProductDataProcessor() {
            this.OverrideRules = new Dictionary<string, string>() {
                { "Starter", "starter" },
                { "Unlimited", "unlimited" }
            };
        }

        public void ProcessData(List<ProductsTemplateResource> productsTemplates, ExtractorParameters extractorParameters)
        {
            if (productsTemplates.IsNullOrEmpty())
            {
                return;
            }

            foreach (var productTemplate in productsTemplates)
            {
                // save Original name for future references
                productTemplate.OriginalName = productTemplate.Name;
                productTemplate.NewName = productTemplate.Name;
                if (extractorParameters.OverrideProductGuids)
                {
                    this.OverrideName(productTemplate);
                }
            }
        }

        public void OverrideName(ProductsTemplateResource template)
        {
            if (this.OverrideRules.IsNullOrEmpty())
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
