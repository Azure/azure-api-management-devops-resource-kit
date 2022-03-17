// --------------------------------------------------------------------------
//  <copyright file="ApiOperationRequestRepresentation.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiOperations
{
    public class ApiOperationRepresentation
    {
        public string ContentType { get; set; }

        public string SchemaId { get; set; }

        public string TypeName { get; set; }
    }
}
