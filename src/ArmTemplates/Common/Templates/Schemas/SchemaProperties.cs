// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Schemas
{
    public class SchemaProperties
    {
        public string Description { get; set; }

        public string SchemaType { get; set; }

        public string Value { get; set; }

        public Object Document { get; set; }
    }
}
