// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors
{
    public class ProductDataProcessor: IProductDataProcessor
    {
        public IDictionary<string, string> OverrideRules {
            get => this.overrideRules;  
            set => this.overrideRules = value;
        }

        IDictionary<string, string> overrideRules;

        public ProductDataProcessor() {
            this.OverrideRules = new Dictionary<string, string>();
            this.OverrideRules.Add("Starter", "starter");
            this.OverrideRules.Add("Unlimited", "unlimited");
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
                        template.Name = rule.Value;
                        break;
                    }
                }
            }
        }
    }
}
