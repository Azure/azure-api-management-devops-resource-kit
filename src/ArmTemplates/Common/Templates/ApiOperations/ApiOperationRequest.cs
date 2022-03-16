// --------------------------------------------------------------------------
//  <copyright file="ApiOperationRequest.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiOperations
{
    public class ApiOperationRequest
    {
        public string Description { get; set; }

        public ApiOperationHeader[] Headers { get; set; }

        public ApiOperationQueryParameter[] QueryParameters { get; set; }

        public ApiOperationRepresentation[] Representations { get; set; }
    }
}
