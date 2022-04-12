// --------------------------------------------------------------------------
//  <copyright file="ParametersTemplateResources.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Parameters
{
    public class ParametersTemplateResources : ITemplateResources
    {
        public List<> Parameters { get; set; }

        public TemplateResource[] BuildTemplateResources()
        {
            throw new NotImplementedException();
        }

        public bool HasContent()
        {
            throw new NotImplementedException();
        }
    }
}
