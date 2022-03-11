// --------------------------------------------------------------------------
//  <copyright file="IAuthorizationServerClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.AuthorizationServer;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions
{
    public interface IAuthorizationServerClient
    {
        Task<List<AuthorizationServerTemplateResource>> GetAllAsync(ExtractorParameters extractorParameters);
    }
}
