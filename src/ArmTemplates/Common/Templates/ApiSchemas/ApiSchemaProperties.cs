// --------------------------------------------------------------------------
//  <copyright file="ApiSchemaProperties.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiSchemas
{
    public class ApiSchemaProperties
    {
        public string ContentType { get; set; }
        
        public ApiSchemaDocument Document { get; set; }
    }
}
