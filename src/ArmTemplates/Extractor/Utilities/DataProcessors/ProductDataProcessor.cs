// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
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

            foreach (var productTemplate in productsTemplates)
            {
                // save Original name for future references
                productTemplate.OriginalName = productTemplate.Name;
                productTemplate.NewName = productTemplate.Name;
                if (extractorParameters.OverrideProductNames)
                {
                    this.OverrideName(productTemplate);
                }
            }

        }


        public void OverrideName(ProductsTemplateResource template)
        {
            if (this.OverrideRules == null)
            {
                return;
            }

            foreach (var rule in this.OverrideRules)
            {
                if (template.Properties.DisplayName.Equals(rule.Key))
                {
                    if (!template.Name.Equals(rule.Value))
                    {
                        template.NewName = rule.Value;
                        template.Name = rule.Value;
                        break;
                    }
                }
            }
        }
    }
}
