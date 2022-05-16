// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction
{
    public interface IProductDataProcessor
    {
        IDictionary<string, string> OverrideRules { get; }
        void ProcessData(List<ProductsTemplateResource> templates, ExtractorParameters extractorParameters);

        void OverrideName(ProductsTemplateResource template);
    }
}
