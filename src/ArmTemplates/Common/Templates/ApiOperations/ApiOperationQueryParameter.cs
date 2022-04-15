// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiOperations
{
    public class ApiOperationQueryParameter
    {
        public string DefaultValue { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public bool Required { get; set; }

        public string SchemaId { get; set; }

        public string Type { get; set; }

        public string TypeName { get; set; }
    }
}
