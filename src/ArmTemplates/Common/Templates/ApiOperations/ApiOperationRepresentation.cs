// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiOperations
{
    public class ApiOperationRepresentation
    {
        public string ContentType { get; set; }

        public string SchemaId { get; set; }

        public string TypeName { get; set; }

        public IDictionary<string, ParameterExampleContract> Examples { get; set; }
    }
}
