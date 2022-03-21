// --------------------------------------------------------------------------
//  <copyright file="IApiOperationExtractor.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiOperations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions
{
    public interface IApiOperationExtractor
    {
        Task<List<ApiOperationTemplateResource>> GenerateApiOperationsResourcesAsync(string apiName, ExtractorParameters extractorParameters);
    }
}
