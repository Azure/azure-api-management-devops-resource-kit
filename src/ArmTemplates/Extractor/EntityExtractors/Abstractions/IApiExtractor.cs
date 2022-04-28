// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions
{
    public interface IApiExtractor
    {
        Task<Template<ApiTemplateResources>> GenerateApiTemplateAsync(string singleApiName, List<string> multipleApiNames, string baseFilesGenerationDirectory, ExtractorParameters extractorParameters);

        Task<ApiTemplateResources> GenerateSingleApiTemplateResourcesAsync(string singleApiName, string baseFilesGenerationDirectory, ExtractorParameters extractorParameters);

        Task<ApiTemplateResources> GetApiRelatedTemplateResourcesAsync(string apiName, string baseFilesGenerationDirectory, ExtractorParameters extractorParameters);
    }
}
