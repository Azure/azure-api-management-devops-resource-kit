// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters
{
    public class ProductConfig : ProductsProperties
    {
        // policy file location (local or url)
        public string Policy { get; set; }
        // coma separated names
        public string Groups { get; set; }
        public List<SubscriptionConfig> Subscriptions { get; set; }
    }
}
