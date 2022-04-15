// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions
{
    public interface ITagExtractor
    {
        Task<Template<TagTemplateResources>> GenerateTagsTemplateAsync(
            string singleApiName, 
            ApiTemplateResources apiTemplateResources, 
            ProductTemplateResources productTemplateResources,
            ExtractorParameters extractorParameters);

        Task<List<TagTemplateResource>> GenerateTagResourcesLinkedToApiAsync(string apiName, ExtractorParameters extractorParameters);

        Task<List<TagTemplateResource>> GenerateTagResourcesLinkedToApiOperationAsync(string apiName, string operationName, ExtractorParameters extractorParameters);
    }
}
