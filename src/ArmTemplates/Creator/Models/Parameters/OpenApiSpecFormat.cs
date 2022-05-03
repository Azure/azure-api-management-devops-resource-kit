// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters
{
    public enum OpenApiSpecFormat
    {
        Unspecified,

        Swagger,
        Swagger_Json = Swagger,

        OpenApi20,
        OpenApi20_Yaml = OpenApi20,
        OpenApi20_Json,

        OpenApi30,
        OpenApi30_Yaml = OpenApi30,
        OpenApi30_Json,
    }
}
