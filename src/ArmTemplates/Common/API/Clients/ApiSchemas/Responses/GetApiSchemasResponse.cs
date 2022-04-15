// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiSchemas;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.ApiSchemas.Responses
{
    public class GetApiSchemasResponse
    {
        [JsonProperty("value")]
        public List<ApiSchemaTemplateResource> Schemas { get; set; }

        public int Count { get; set; }
    }
}
