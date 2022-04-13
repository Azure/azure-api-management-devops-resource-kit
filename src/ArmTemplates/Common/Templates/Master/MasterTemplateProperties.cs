// --------------------------------------------------------------------------
//  <copyright file="MasterTemplateProperties.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Master
{
    public class MasterTemplateProperties
    {
        public string Mode { get; set; }
        public MasterTemplateLink TemplateLink { get; set; }
        public Dictionary<string, TemplateParameterProperties> Parameters { get; set; }
    }
}
