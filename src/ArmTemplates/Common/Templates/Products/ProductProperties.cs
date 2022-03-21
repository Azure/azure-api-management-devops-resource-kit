// --------------------------------------------------------------------------
//  <copyright file="ProductProperties.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products
{
    public class ProductsProperties
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Terms { get; set; }
        public bool SubscriptionRequired { get; set; }
        public bool? ApprovalRequired { get; set; }
        public int? SubscriptionsLimit { get; set; }
        public string State { get; set; }
        public string DisplayName { get; set; }
    }
}
