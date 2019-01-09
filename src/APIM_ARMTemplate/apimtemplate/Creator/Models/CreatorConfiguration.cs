using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class CLICreatorArguments
    {
        public string configFile { get; set; }
    }

    public class CreatorConfig
    {
        public string version { get; set; }
        public string apimServiceName { get; set; }
        public APIVersionSetConfig apiVersionSet { get; set; }
        public APIConfig api { get; set; }
        public string outputLocation { get; set; }
        public bool linked { get; set; }
    }

    public class APIConfig
    {
        // openApiSpec file location (local or url)
        public string openApiSpec { get; set; }
        // policy file location (local or url)
        public string policy { get; set; }
        public string suffix { get; set; }
        public string versionSetId { get; set; }
        public string apiVersion { get; set; }
        public string apiVersionDescription { get; set; }
        public string revision { get; set; }
        public string revisionDescription { get; set; }
        public Dictionary<string, OperationsConfig> operations { get; set; }
        public APITemplateAuthenticationSettings authenticationSettings { get; set; }
        public string products { get; set; }
    }

    public class APIVersionSetConfig : APIVersionSetProperties
    {
        public string id { get; set; }
    }

    public class OperationsConfig
    {
        // policy file location (local or url)
        public string policy { get; set; }
    }

}
