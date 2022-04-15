﻿// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Master
{
    public class MasterTemplateResource : TemplateResource
    {
        public MasterTemplateProperties Properties { get; set; }
    }
}
