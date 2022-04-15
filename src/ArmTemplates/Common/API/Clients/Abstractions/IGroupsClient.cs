// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Groups;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions
{
    public interface IGroupsClient
    {
        Task<List<GroupTemplateResource>> GetAllLinkedToProductAsync(string productName, ExtractorParameters extractorParameters);

        Task<List<GroupTemplateResource>> GetAllAsync(ExtractorParameters extractorParameters);
    }
}
