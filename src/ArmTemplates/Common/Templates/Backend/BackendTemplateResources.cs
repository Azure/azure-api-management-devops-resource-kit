// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend
{
    public class BackendTemplateResources : ITemplateResources
    {
        public List<BackendTemplateResource> Backends { get; set; } = new();

        public IDictionary<string, BackendApiParameters> BackendNameParametersCache { get; set; } = new Dictionary<string, BackendApiParameters>();

        public TemplateResource[] BuildTemplateResources()
        {
            return this.Backends.ToArray();
        }

        public bool HasContent()
        {
            return !this.Backends.IsNullOrEmpty();
        }
    }
}
