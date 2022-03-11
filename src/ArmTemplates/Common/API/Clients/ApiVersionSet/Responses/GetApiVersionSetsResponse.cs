// --------------------------------------------------------------------------
//  <copyright file="GetApiVersionSetsResponse.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiVersionSet;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.ApiVersionSet.Responses
{
    public class GetApiVersionSetsResponse
    {
        [JsonProperty("value")]
        public List<ApiVersionSetTemplateResource> ApiVersionSets { get; set; }

        public int Count { get; set; }
    }
}
