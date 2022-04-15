// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiRevisions;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.ApiRevision.Responses
{
    public class GetApiRevisionsResponse
    {
        [JsonProperty("value")]
        public List<ApiRevisionTemplateResource> ApiRevisions { get; set; }

        public int Count { get; set; }
    }
}
