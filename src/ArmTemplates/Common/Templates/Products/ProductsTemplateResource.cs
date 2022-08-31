// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Newtonsoft.Json;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products
{
    public class ProductsTemplateResource : TemplateResource
    {
        [JsonIgnore]
        public string NewName { get; set; }

        public ProductsProperties Properties { get; set; }
    }
}
