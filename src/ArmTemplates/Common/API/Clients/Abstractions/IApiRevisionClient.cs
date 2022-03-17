// --------------------------------------------------------------------------
//  <copyright file="IApiRevisionClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiRevisions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions
{
    public interface IApiRevisionClient
    {
        Task<List<ApiRevisionTemplateResource>> GetApiRevisionsAsync(string apiName, ExtractorParameters extractorParameters);
    }
}
