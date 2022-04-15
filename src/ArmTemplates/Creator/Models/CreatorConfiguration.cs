// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiVersionSet;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.AuthorizationServer;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models
{
    public class CLICreatorArguments
    {
        public string appInsightsInstrumentationKey { get; set; }
        public string appInsightsName { get; set; }
        public string configFile { get; set; }

        public string backendurlconfigFile { get; set; }

        public string preferredAPIsForDeployment { get; set; }
    }

    public class CreatorConfig
    {
        public bool ConsiderAllApiForDeployments { get; set; } = true;

        public string[] PreferredApis { get; set; }

        public string version { get; set; }
        public string apimServiceName { get; set; }
        // policy file location (local or url)
        public string policy { get; set; }
        public List<APIVersionSetConfig> apiVersionSets { get; set; }
        public List<APIConfig> apis { get; set; }
        public List<ProductConfig> products { get; set; }
        public List<PropertyConfig> namedValues { get; set; }
        public List<LoggerConfig> loggers { get; set; }
        public List<AuthorizationServerProperties> authorizationServers { get; set; }
        public List<BackendTemplateProperties> backends { get; set; }
        public List<TagProperties> tags { get; set; }
        public List<SubscriptionConfig> subscriptions { get; set; }
        public string outputLocation { get; set; }
        public bool linked { get; set; }
        public string linkedTemplatesBaseUrl { get; set; }
        public string linkedTemplatesUrlQueryString { get; set; }
        public string baseFileName { get; set; }

        public bool parameterizeNamedValues { get; set; }
        public List<ServiceUrlProperty> serviceUrlParameters { get; set; }
    }

    public class APIVersionSetConfig : ApiVersionSetProperties
    {
        public string id { get; set; }
    }

    public class APIConfig
    {
        // used to build displayName and resource name from APITemplateResource schema
        public string name { get; set; }
        // optional : overrides title from OpenAPI definition
        public string displayName { get; set; }
        public string description { get; set; }
        public string serviceUrl { get; set; }
        // used to build type and apiType from APITemplateResource schema
        public string type { get; set; }
        // openApiSpec file location (local or url), used to build protocols, value, and format from APITemplateResource schema
        public string openApiSpec { get; set; }
        // format of the API definition.
        public OpenApiSpecFormat openApiSpecFormat { get; set; }
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
        public APITemplateSubscriptionKeyParameterNames subscriptionKeyParameterNames { get; set; }
        public string products { get; set; }
        public string tags { get; set; }
        public string protocols { get; set; }
        public DiagnosticConfig diagnostic { get; set; }
        // does not currently include subscriptionKeyParameterNames, sourceApiId, and wsdlSelector from APITemplateResource schema
    }

    public enum OpenApiSpecFormat
    {
        Unspecified,

        Swagger,
        Swagger_Json = Swagger,

        OpenApi20,
        OpenApi20_Yaml = OpenApi20,
        OpenApi20_Json,

        OpenApi30,
        OpenApi30_Yaml = OpenApi30,
        OpenApi30_Json,
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

    public class ProductConfig : ProductsProperties
    {
        // policy file location (local or url)
        public string policy { get; set; }
        // coma separated names
        public string groups { get; set; }
        public List<SubscriptionConfig> subscriptions { get; set; }
    }

    public class PropertyConfig : NamedValueProperties
    {

    }

    public class SubscriptionConfig : SubscriptionsTemplateProperties
    {
        public string name { get; set; }
    }
}
