// --------------------------------------------------------------------------
//  <copyright file="IProductApisApiClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions
{
    public interface IProductsClient
    {
        Task<List<ProductApiTemplateResource>> GetAllLinkedToApiAsync(string apiName, ExtractorParameters extractorParameters);

        Task<List<ProductsTemplateResource>> GetAllAsync(ExtractorParameters extractorParameters);
    }
}
