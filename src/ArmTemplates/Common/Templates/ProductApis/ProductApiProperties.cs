// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis
{
    public class ProductApiProperties
    {
        public string DisplayName { get; set; }

        public string Description { get; set; }

        public bool SubscriptionRequired { get; set; }
        
        public bool ApprovalRequired { get; set; }

        public string State { get; set; }
    }
}
