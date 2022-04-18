// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.NamedValues.Responses
{
    class GetNamedValuesResponse
    {
        [JsonProperty("value")]
        public List<NamedValueTemplateResource> NamedValues { get; set; }

        public int Count { get; set; }
    }
}
