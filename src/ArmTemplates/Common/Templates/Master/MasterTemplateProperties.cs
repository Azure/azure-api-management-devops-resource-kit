// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
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
