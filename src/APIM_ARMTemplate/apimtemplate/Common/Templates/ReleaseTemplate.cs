using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{

    public class ReleaseTemplate : APITemplateResource
    {
        public string name { get; set; }
        public string type { get; set; }
        public string apiVersion { get; set; }
        public ReleasTemplateProperties properties { get; set; }
    }

    public class ReleasTemplateProperties
    {
        public string apiId { get; set; }
        public string notes { get; set; }
    }
}
