// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Product.Responses
{
    public class GetAllProductsResponse
    {
        [JsonProperty("value")]
        public List<ProductsTemplateResource> Products { get; set; }

        public int Count { get; set; }
    }
}
