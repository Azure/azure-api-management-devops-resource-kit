using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class CLICreatorArguments
    {
        public string configFile { get; set; }
    }

    public class CreatorConfig
    {
        public string version { get; set; }
        public string apimServiceName { get; set; }
        public List<APIVersionSetConfig> apiVersionSets { get; set; }
        public List<APIConfig> apis { get; set; }
        public List<ProductsTemplateProperties> products { get; set; }
        public List<LoggerConfig> loggers { get; set; }
        public string outputLocation { get; set; }
        public bool linked { get; set; }
        public string linkedTemplatesBaseUrl { get; set; }
    }

    public class APIVersionSetConfig: APIVersionSetProperties
    {
        public string id { get; set; }
    }

    public class APIConfig
    {
        public string name { get; set; }
        // openApiSpec file location (local or url)
        public string openApiSpec { get; set; }
        // policy file location (local or url)
        public string policy { get; set; }
        public string suffix { get; set; }
        public bool subscriptionRequired { get; set; }
        public string apiVersion { get; set; }
        public string apiVersionDescription { get; set; }
        public string apiVersionSetId { get; set; }
        public string revision { get; set; }
        public string revisionDescription { get; set; }
        public Dictionary<string, OperationsConfig> operations { get; set; }
        public APITemplateAuthenticationSettings authenticationSettings { get; set; }
        public string products { get; set; }
        public DiagnosticConfig diagnostic { get; set; }
    }

    public class OperationsConfig
    {
        // policy file location (local or url)
        public string policy { get; set; }
    }

    public class DiagnosticConfig : DiagnosticTemplateProperties
    {
        public string name { get; set; }
    }

    public class LoggerConfig : LoggerTemplateProperties
    {
        public string name { get; set; }
    }


}
