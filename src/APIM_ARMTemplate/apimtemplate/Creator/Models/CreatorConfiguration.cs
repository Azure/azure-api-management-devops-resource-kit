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
        public List<ProductConfig> products { get; set; }
        public List<LoggerConfig> loggers { get; set; }
        public List<AuthorizationServerTemplateProperties> authorizationServers { get; set; }
        public List<BackendTemplateProperties> backends { get; set; }
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
        // used to build displayName and resource name from APITemplateResource schema
        public string name { get; set; }
        public string description { get; set; }
        public string serviceUrl { get; set; }
        // used to build type and apiType from APITemplateResource schema
        public string type { get; set; }
        // openApiSpec file location (local or url), used to build protocols, value, and format from APITemplateResource schema
        public string openApiSpec { get; set; }
        // policy file location (local or url)
        public string policy { get; set; }
        // used to buld path from APITemplateResource schema
        public string suffix { get; set; }
        public bool subscriptionRequired { get; set; }
        public bool isCurrent { get; set; }
        public string apiVersion { get; set; }
        public string apiVersionDescription { get; set; }
        public string apiVersionSetId { get; set; }
        public string apiRevision { get; set; }
        public string apiRevisionDescription { get; set; }
        public Dictionary<string, OperationsConfig> operations { get; set; }
        public APITemplateAuthenticationSettings authenticationSettings { get; set; }
        public string products { get; set; }
        public string protocols { get; set; }
        public DiagnosticConfig diagnostic { get; set; }
        // does not currently include subscriptionKeyParameterNames, sourceApiId, and wsdlSelector from APITemplateResource schema
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

    public class ProductConfig : ProductsTemplateProperties
    {
        // policy file location (local or url)
        public string policy { get; set; }
    }
    
}
