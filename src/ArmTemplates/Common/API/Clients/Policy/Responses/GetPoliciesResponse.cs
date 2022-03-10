// --------------------------------------------------------------------------
//  <copyright file="GetPoliciesResponse.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Policy.Responses
{
    public class GetPoliciesResponse
    {
        [JsonProperty("value")]
        public List<PolicyTemplateResource> Policies { get; set; }

        public int Count { get; set; }
    }
}
