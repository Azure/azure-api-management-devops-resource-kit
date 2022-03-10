// --------------------------------------------------------------------------
//  <copyright file="TagTemplateResource.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags
{
    public class TagTemplateResource : TemplateResource
    {
        public TagProperties Properties { get; set; }
    }
}
