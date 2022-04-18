// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Tags.Responses
{
    public class GetTagsResponse
    {
        [JsonProperty("value")]
        public List<TagTemplateResource> Tags { get; set; }

        public int Count { get; set; }
    }
}
