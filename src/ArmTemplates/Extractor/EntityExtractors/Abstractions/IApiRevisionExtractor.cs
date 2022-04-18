// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiRevisions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions
{
    public interface IApiRevisionExtractor
    {
        IAsyncEnumerable<ApiRevisionTemplateResource> GetApiRevisionsAsync(string apiName, ExtractorParameters extractorParameters);

        Task<Template<ApiTemplateResources>> GenerateApiRevisionTemplateAsync(
            string currentRevision,
            List<string> revList,
            string baseFilesGenerationDirectory,
            ExtractorParameters extractorParameters);
    }
}
