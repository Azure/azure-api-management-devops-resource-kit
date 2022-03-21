// --------------------------------------------------------------------------
//  <copyright file="ApiSchemaDocument.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiSchemas
{
    public class ApiSchemaDocument
    {
        public object Components { get; set; }

        public object Definitions { get; set; }

        public string Value { get; set; }
    }
}
