// --------------------------------------------------------------------------
//  <copyright file="GetAllServiceApiProductsResponse.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.ProductApis.Responses
{
    class GetAllServiceApiProductsResponse
    {
        [JsonProperty("value")]
        public List<ServiceApisProductTemplateResource> ServiceApisProducts { get; set; }

        public int Count { get; set; }
    }
}
