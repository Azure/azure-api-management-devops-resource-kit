// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions
{
    public interface INamedValuesClient
    {
        Task<List<NamedValueTemplateResource>> GetAllAsync(ExtractorParameters extractorParameters);

        Task<NamedValuesSecretValue> ListNamedValueSecretValueAsync(string namedValueId, ExtractorParameters extractorParameters);
    }
}
