// --------------------------------------------------------------------------
//  <copyright file="GetAllServiceApiProductsResponse.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Product.Responses
{
    class GetAllProductsLinkedToApiResponse
    {
        [JsonProperty("value")]
        public List<ProductApiTemplateResource> ProductApis { get; set; }

        public int Count { get; set; }
    }
}
