// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues
{
    public class NamedValuesResources : ITemplateResources
    {
        public List<NamedValueTemplateResource> NamedValues { get; set; } = new();

        public TemplateResource[] BuildTemplateResources()
        {
            return this.NamedValues.ToArray();
        }

        public bool HasContent()
        {
            return !this.NamedValues.IsNullOrEmpty();
        }
    }
}
