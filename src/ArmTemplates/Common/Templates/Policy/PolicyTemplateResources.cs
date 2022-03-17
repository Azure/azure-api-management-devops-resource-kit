// --------------------------------------------------------------------------
//  <copyright file="PolicyTemplateResources.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy
{
    public class PolicyTemplateResources : ITemplateResources
    {
        public PolicyTemplateResource GlobalServicePolicy { get; set; }

        public bool HasContent()
        {
            return this.GlobalServicePolicy is not null;
        }

        public TemplateResource[] BuildTemplateResources()
        {
            return new[] { this.GlobalServicePolicy };
        }
    }
}
