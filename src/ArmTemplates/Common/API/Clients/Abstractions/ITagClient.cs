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
        Task<string> GetAllAsync(ExtractorParameters extractorParameters, int skipAmountOfRecords = 0);

        Task<List<TagTemplateResource>> GetAllTagsLinkedToApiAsync(string apiName, ExtractorParameters extractorParameters);

        Task<List<TagTemplateResource>> GetAllTagsLinkedToProductAsync(string productName, ExtractorParameters extractorParameters);
    }
}
