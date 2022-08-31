// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiManagementService
{
    public class ApiManagementServiceResource : TemplateResource
    {
        public ApiManagementServiceIdentity Identity { get; set; }

        public string Location { get; set; }

        public IDictionary<string, string> Tags { get; set; }

        public string[] Zones { get; set; }

        public ApiManagementServiceSkuProperties Sku { get; set; }

        public ApiManagementServiceProperties Properties { get; set; }
    }
}
