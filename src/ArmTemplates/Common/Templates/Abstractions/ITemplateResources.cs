// --------------------------------------------------------------------------
//  <copyright file="ITemplateResources.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions
{
    public interface ITemplateResources
    {
        TemplateResource[] BuildTemplateResources();

        bool HasContent();
    }
}
