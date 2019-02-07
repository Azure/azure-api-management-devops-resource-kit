using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class Parameters
    {
        public ApimServiceName ApimServiceName { get; set; }
        public RepoBaseUrl repoBaseUrl { get; set; }
    }
    public class ApimServiceName
    {
        public string type { get; set; }
    }

    public class Metadata
    {
        public string description { get; set; }
    }

    public class RepoBaseUrl
    {
        public string type { get; set; }
        public Metadata metadata { get; set; }
    }
}
