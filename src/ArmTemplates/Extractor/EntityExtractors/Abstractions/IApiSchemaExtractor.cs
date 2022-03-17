// --------------------------------------------------------------------------
//  <copyright file="ISchemaExtractor.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiSchemas;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions
{
    public interface IApiSchemaExtractor
    {
        Task<ApiSchemaTemplateResources> GenerateApiSchemaResourcesAsync(string apiName, ExtractorParameters extractorParameters);
    }
}
