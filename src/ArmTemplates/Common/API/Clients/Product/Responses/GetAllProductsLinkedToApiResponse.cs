// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Product.Responses
{
    public class GetAllProductsLinkedToApiResponse
    {
        [JsonProperty("value")]
        public List<ProductApiTemplateResource> ProductApis { get; set; }

        public int Count { get; set; }
    }
}
