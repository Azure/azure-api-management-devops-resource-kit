// --------------------------------------------------------------------------
//  <copyright file="IApiSchemasClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiSchemas;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions
{
    public interface IApiSchemaClient
    {
        Task<List<ApiSchemaTemplateResource>> GetApiSchemasAsync(string apiName, ExtractorParameters extractorParameters);
    }
}
