using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{

    public class ReleaseTemplateResource : APITemplateSubResource
    {
        public ReleasTemplateProperties properties { get; set; }
    }

    public class ReleasTemplateProperties
    {
        public string apiId { get; set; }
        public string notes { get; set; }
    }
}
