using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    public class GroupTemplateResource : TemplateResource
    {
        public GroupResourceProperties properties { get; set; }
    }

    public class GroupResourceProperties
    {
        public string type { get; set; }
        public string description { get; set; }
        public string displayName { get; set; }
        public string externalId { get; set; }

    }
}
