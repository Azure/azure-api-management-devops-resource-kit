// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions
{
    public interface IProductApisExtractor
    {
        Task<Template<ProductApiTemplateResources>> GenerateProductApisTemplateAsync(
            string singleApiName, 
            List<string> multipleApiNames, 
            ExtractorParameters extractorParameters);

        Task<List<ProductApiTemplateResource>> GenerateSingleApiTemplateAsync(
            string singleApiName,
            ExtractorParameters extractorParameters,
            bool addDependsOnParameter = false);
    }
}
