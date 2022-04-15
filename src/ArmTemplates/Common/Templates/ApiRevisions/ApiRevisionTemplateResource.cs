// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiRevisions
{
    public class ApiRevisionTemplateResource
    {
        public string Id { get; set; }

        public string ApiId { get; set; }

        public string ApiRevision { get; set; }

        public string Description { get; set; }

        public string PrivateUrl { get; set; }

        public bool IsOnline { get; set; }

        public bool IsCurrent { get; set; }
    }
}
