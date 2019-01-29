using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract.Operation
{
    public class Properties
    {
        public string policyContent { get; set; }
    }

    public class Value
    {
        public string id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public Properties properties { get; set; }
    }

    public class Operation
    {
        public List<Value> value { get; set; }
    }
}
