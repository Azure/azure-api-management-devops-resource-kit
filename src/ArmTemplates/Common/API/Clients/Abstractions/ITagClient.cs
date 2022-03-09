// --------------------------------------------------------------------------
//  <copyright file="ITagClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions
{
    public interface ITagClient
    {
        Task<string> GetAllAsync(ExtractorParameters extractorParameters, int skipNumOfRecords = 0);

        Task<List<TagTemplateResource>> GetAllTagsLinkedToApiAsync(ExtractorParameters extractorParameters, string apiName);

        Task<List<TagTemplateResource>> GetAllTagsLinkedToProductAsync(ExtractorParameters extractorParameters, string productName);
    }
}
