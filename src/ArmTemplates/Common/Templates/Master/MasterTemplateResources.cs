// --------------------------------------------------------------------------
//  <copyright file="MasterTemplateResources.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Master
{
    public class MasterTemplateResources : ITemplateResources
    {
        public List<MasterTemplateResource> DeploymentResources { get; set; } = new();

        public TemplateResource[] BuildTemplateResources()
        {
            return this.DeploymentResources.ToArray();
        }

        public bool HasContent()
        {
            return !this.DeploymentResources.IsNullOrEmpty();
        }
    }
}
