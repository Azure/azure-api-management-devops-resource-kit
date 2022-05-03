// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters
{
    public class LoggerConfig : LoggerTemplateProperties
    {
        public string Name { get; set; }
    }
}
