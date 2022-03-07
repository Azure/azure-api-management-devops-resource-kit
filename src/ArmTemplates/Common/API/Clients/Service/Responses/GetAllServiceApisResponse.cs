// --------------------------------------------------------------------------
//  <copyright file="GetAllProductApisResponse.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Service;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Service.Responses
{
    public class GetAllServiceApisResponse
    {
        [JsonProperty("value")]
        public List<ServiceApiTemplateResource> ServiceApis { get; set; }

        public int Count { get; set; }
    }
}
