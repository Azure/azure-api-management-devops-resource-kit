// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels
{
    public class SubscriptionsTemplateResource : TemplateResource
    {
        public SubscriptionsTemplateProperties Properties { get; set; }
    }

    public class SubscriptionsTemplateProperties
    {
        public string ownerId { get; set; }
        public string scope { get; set; }
        public string displayName { get; set; }
        public string primaryKey { get; set; }
        public string secondaryKey { get; set; }
        public string state { get; set; }
        public bool? allowTracing { get; set; }
    }
}