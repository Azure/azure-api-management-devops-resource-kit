// --------------------------------------------------------------------------
//  <copyright file="IApiOperationClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiOperations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions
{
    public interface IApiOperationClient
    {
        Task<List<ApiOperationTemplateResource>> GetOperationsLinkedToApiAsync(string apiName, ExtractorParameters extractorParameters);
    }
}
