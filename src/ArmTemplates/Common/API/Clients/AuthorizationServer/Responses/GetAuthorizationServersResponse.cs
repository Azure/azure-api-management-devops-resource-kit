// --------------------------------------------------------------------------
//  <copyright file="GetAuthorizationServersResponse.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.AuthorizationServer;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.AuthorizationServer.Responses
{
    class GetAuthorizationServersResponse
    {
        [JsonProperty("value")]
        public List<AuthorizationServerTemplateResource> AuthorizationServers { get; set; }

        public int Count { get; set; }
    }
}
