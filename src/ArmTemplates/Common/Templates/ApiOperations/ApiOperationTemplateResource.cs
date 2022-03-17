// --------------------------------------------------------------------------
//  <copyright file="ApiOperationTemplateResource.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiOperations
{
    public class ApiOperationTemplateResource : TemplateResource
    {
        [JsonIgnore]
        public string OriginalName { get; set; }

        public ApiOperationProperties Properties { get; set; }
    }
}
