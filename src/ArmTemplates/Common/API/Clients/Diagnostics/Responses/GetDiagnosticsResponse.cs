// --------------------------------------------------------------------------
//  <copyright file="GetDiagnosticsResponse.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Diagnostics.Responses
{
    class GetDiagnosticsResponse
    {
        [JsonProperty("value")]
        public List<DiagnosticTemplateResource> Diagnostics { get; set; }

        public int Count { get; set; }
    }
}
