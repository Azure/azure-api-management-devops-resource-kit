// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis
{
    public class ApiTemplateResource : TemplateResource
    {
        [JsonIgnore]
        public string OriginalName { get; set; }

        [JsonIgnore]
        public string ApiNameWithRevision { get; set; }

        public ApiProperties Properties { get; set; }
    }
}
