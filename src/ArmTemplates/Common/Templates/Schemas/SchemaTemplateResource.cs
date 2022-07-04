// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Schemas
{
    public class SchemaTemplateResource: TemplateResource
    {
        [JsonIgnore]
        public string OriginalName { get; set; }

        public SchemaProperties Properties { get; set; }
    }
}
