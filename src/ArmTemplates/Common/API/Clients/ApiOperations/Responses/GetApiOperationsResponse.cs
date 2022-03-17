// --------------------------------------------------------------------------
//  <copyright file="GetApiOperationsResponse.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiOperations;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.ApiOperations.Responses
{
    class GetApiOperationsResponse
    {
        [JsonProperty("value")]
        public List<ApiOperationTemplateResource> ApiOperations { get; set; }

        public int Count { get; set; }
    }
}
