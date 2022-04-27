// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Models
{
    public class AzurePagedResponse<T>
    {
        [JsonProperty("value")]
        public List<T> Items { get; set; }

        public int Count { get; set; }

        /// <summary>
        /// Represents url of request that needs to be called for retrieving next page of responses
        /// </summary>
        public string NextLink { get; set; }
    }
}
