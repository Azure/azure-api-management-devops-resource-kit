// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Groups
{
    public class GroupProperties
    {
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string Type { get; set; }
        public string ExternalId { get; set; }
        public bool BuiltIn { get; set; }
    }
}
