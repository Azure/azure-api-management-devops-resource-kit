// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Product
{
    public class ProductsClient : ApiClientBase, IProductsClient
    {
        const string GetAllProductsLinkedToApiRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/products?api-version={5}";
        const string GetAllProductsRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/products?api-version={4}";

        public async Task<List<ProductsTemplateResource>> GetAllAsync(ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(GetAllProductsRequest,
                this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, GlobalConstants.ApiVersion);

            return await this.GetPagedResponseAsync<ProductsTemplateResource>(azToken, requestUrl);
        }

        public async Task<List<ProductApiTemplateResource>> GetAllLinkedToApiAsync(string apiName, ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(GetAllProductsLinkedToApiRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, apiName, GlobalConstants.ApiVersion);

            return await this.GetPagedResponseAsync<ProductApiTemplateResource>(azToken, requestUrl);
        }
    }
}
