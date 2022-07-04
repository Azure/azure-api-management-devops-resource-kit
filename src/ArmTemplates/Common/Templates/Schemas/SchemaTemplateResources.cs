// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Schemas
{
    public class SchemaTemplateResources : ITemplateResources
    {
        public List<SchemaTemplateResource> Schemas { get; set; } = new();

        public TemplateResource[] BuildTemplateResources()
        {
            return this.Schemas.ToArray();
        }

        public bool HasContent()
        {
            return !this.Schemas.IsNullOrEmpty();
        }
    }
}
