using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{

    public class PolicyTemplate : APITemplateResource
    {
        public string name { get; set; }
        public string type { get; set; }
        public string apiVersion { get; set; }
        public PolicyTemplateProperties properties { get; set; }
    }

    public class PolicyTemplateProperties
    {
        public string policyContent { get; set; }
        public string contentFormat { get; set; }
    }
}
