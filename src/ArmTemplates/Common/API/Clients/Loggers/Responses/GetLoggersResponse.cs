// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Loggers.Responses
{
    public class GetLoggersResponse
    {
        [JsonProperty("value")]
        public List<LoggerTemplateResource> Loggers { get; set; }

        public int Count { get; set; }
    }
}
