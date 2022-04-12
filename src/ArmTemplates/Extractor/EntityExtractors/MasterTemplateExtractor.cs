using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiVersionSet;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.TagApi;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.AuthorizationServer;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger.Cache;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class MasterTemplateExtractor : ApiExtractor, IMasterTemplateExtractor
    {
        readonly ITemplateBuilder templateBuilder;
        readonly IPolicyExtractor policyExtractor;
        readonly INamedValuesClient namedValuesClient;

        public MasterTemplateExtractor(
            ILogger<NamedValuesExtractor> namedValuesExtractorLogger,
            ILogger<ApiExtractor> logger,
            ITemplateBuilder templateBuilder,
            IApisClient apisClient,
            INamedValuesClient namedValuesClient,
            IDiagnosticExtractor diagnosticExtractor,
            IApiSchemaExtractor apiSchemaExtractor,
            IPolicyExtractor policyExtractor,
            IProductApisExtractor productApisExtractor,
            ITagExtractor tagExtractor,
            IApiOperationExtractor apiOperationExtractor)
            : base(logger, templateBuilder, apisClient, diagnosticExtractor, apiSchemaExtractor, policyExtractor, productApisExtractor, tagExtractor, apiOperationExtractor)
        {
            this.templateBuilder = templateBuilder;
            this.policyExtractor = policyExtractor;
            this.namedValuesClient = namedValuesClient;
        }

        public Template GenerateLinkedMasterTemplate(
            Template<ApiTemplateResources> apiTemplate,
            Template<PolicyTemplateResources> globalServicePolicyTemplate,
            Template<ApiVersionSetTemplateResources> apiVersionSetTemplate,
            Template<ProductTemplateResources> productsTemplate,
            Template<ProductApiTemplateResources> productAPIsTemplate,
            Template<TagApiTemplateResources> apiTagsTemplate,
            Template loggersTemplate,
            Template backendsTemplate,
            Template<AuthorizationServerTemplateResources> authorizationServersTemplate,
            Template namedValuesTemplate,
            Template<TagTemplateResources> tagTemplate,
            FileNames fileNames,
            ExtractorParameters extractorParameters)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Generating master template");
            // create empty template
            Template masterTemplate = this.templateBuilder.GenerateEmptyTemplate().Build();

            // add parameters
            masterTemplate.Parameters = this.CreateMasterTemplateParameters(true, extractorParameters);

            // add deployment resources that links to all resource files
            List<TemplateResource> resources = new List<TemplateResource>();

            // namedValue
            string namedValueDeploymentResourceName = "namedValuesTemplate";
            // all other deployment resources will depend on named values
            string[] dependsOnNamedValues = Array.Empty<string>();

            // api dependsOn
            List<string> apiDependsOn = new List<string>();
            List<string> productAPIDependsOn = new List<string>();
            List<string> apiTagDependsOn = new List<string>();

            if (namedValuesTemplate != null && namedValuesTemplate.Resources.Count() != 0)
            {
                dependsOnNamedValues = new string[] { $"[resourceId('Microsoft.Resources/deployments', '{namedValueDeploymentResourceName}')]" };
                apiDependsOn.Add($"[resourceId('Microsoft.Resources/deployments', '{namedValueDeploymentResourceName}')]");
                string namedValuesUri = this.GenerateLinkedTemplateUri(extractorParameters.LinkedTemplatesUrlQueryString, extractorParameters.LinkedTemplatesSasToken, fileNames.NamedValues);
                resources.Add(this.CreateLinkedMasterTemplateResourceForPropertyTemplate(namedValueDeploymentResourceName, namedValuesUri, Array.Empty<string>(), extractorParameters));
            }

            // globalServicePolicy
            if (globalServicePolicyTemplate != null && globalServicePolicyTemplate.Resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'globalServicePolicyTemplate')]");
                string globalServicePolicyUri = this.GenerateLinkedTemplateUri(extractorParameters.LinkedTemplatesUrlQueryString, extractorParameters.LinkedTemplatesSasToken, fileNames.GlobalServicePolicy);
                resources.Add(this.CreateLinkedMasterTemplateResourceWithPolicyToken("globalServicePolicyTemplate", globalServicePolicyUri, dependsOnNamedValues, extractorParameters));
            }

            // apiVersionSet
            if (apiVersionSetTemplate != null && apiVersionSetTemplate.Resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'versionSetTemplate')]");
                string apiVersionSetUri = this.GenerateLinkedTemplateUri(extractorParameters.LinkedTemplatesUrlQueryString, extractorParameters.LinkedTemplatesSasToken, fileNames.ApiVersionSets);
                resources.Add(this.CreateLinkedMasterTemplateResource("versionSetTemplate", apiVersionSetUri, dependsOnNamedValues));
            }

            // product
            if (productsTemplate != null && productsTemplate.Resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'productsTemplate')]");
                productAPIDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'productsTemplate')]");
                string productsUri = this.GenerateLinkedTemplateUri(extractorParameters.LinkedTemplatesUrlQueryString, extractorParameters.LinkedTemplatesSasToken, fileNames.Products);
                var templateResource = this.CreateLinkedMasterTemplateResource("productsTemplate", productsUri, dependsOnNamedValues);
                if (extractorParameters.PolicyXMLBaseUrl != null)
                {
                    templateResource.Properties.parameters.Add(ParameterNames.PolicyXMLBaseUrl, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.PolicyXMLBaseUrl}')]" });
                }
                if (extractorParameters.PolicyXMLSasToken != null)
                {
                    templateResource.Properties.parameters.Add(ParameterNames.PolicyXMLSasToken, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.PolicyXMLSasToken}')]" });
                }
                resources.Add(templateResource);
            }

            if (tagTemplate != null && tagTemplate.Resources.Count() != 0)
            {
                apiTagDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'tagTemplate')]");
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'tagTemplate')]");
                string tagUri = this.GenerateLinkedTemplateUri(extractorParameters.LinkedTemplatesUrlQueryString, extractorParameters.LinkedTemplatesSasToken, fileNames.Tags);
                resources.Add(this.CreateLinkedMasterTemplateResource("tagTemplate", tagUri, dependsOnNamedValues));
            }

            // logger
            if (loggersTemplate != null && loggersTemplate.Resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'loggersTemplate')]");
                string loggersUri = this.GenerateLinkedTemplateUri(extractorParameters.LinkedTemplatesUrlQueryString, extractorParameters.LinkedTemplatesSasToken, fileNames.Loggers);
                resources.Add(this.CreateLinkedMasterTemplateResourceForLoggerTemplate("loggersTemplate", loggersUri, dependsOnNamedValues, extractorParameters));
            }

            // backend
            if (backendsTemplate != null && backendsTemplate.Resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'backendsTemplate')]");
                string backendsUri = this.GenerateLinkedTemplateUri(extractorParameters.LinkedTemplatesUrlQueryString, extractorParameters.LinkedTemplatesSasToken, fileNames.Backends);
                var templateResource = this.CreateLinkedMasterTemplateResource("backendsTemplate", backendsUri, dependsOnNamedValues);
                if (extractorParameters.ParameterizeBackend)
                {
                    templateResource.Properties.parameters.Add(ParameterNames.BackendSettings,
                        new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.BackendSettings}')]" });
                }

                resources.Add(templateResource);
            }

            // authorizationServer
            if (authorizationServersTemplate != null && authorizationServersTemplate.Resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'authorizationServersTemplate')]");
                string authorizationServersUri = this.GenerateLinkedTemplateUri(extractorParameters.LinkedTemplatesUrlQueryString, extractorParameters.LinkedTemplatesSasToken, fileNames.AuthorizationServers);
                resources.Add(this.CreateLinkedMasterTemplateResource("authorizationServersTemplate", authorizationServersUri, dependsOnNamedValues));
            }

            // api
            if (apiTemplate != null && apiTemplate.Resources.Count() != 0)
            {
                apiTagDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'apisTemplate')]");
                productAPIDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'apisTemplate')]");
                string apisUri = this.GenerateLinkedTemplateUri(extractorParameters.LinkedTemplatesUrlQueryString, extractorParameters.LinkedTemplatesSasToken, apiTemplate.TypedResources.FileName);
                resources.Add(this.CreateLinkedMasterTemplateResourceForApiTemplate("apisTemplate", apisUri, apiDependsOn.ToArray(), extractorParameters));
            }

            // productAPIs
            if (productAPIsTemplate != null && productAPIsTemplate.Resources.Count() != 0)
            {
                string productAPIsUri = this.GenerateLinkedTemplateUri(extractorParameters.LinkedTemplatesUrlQueryString, extractorParameters.LinkedTemplatesSasToken, fileNames.ProductAPIs);

                resources.Add(this.CreateLinkedMasterTemplateResource("productAPIsTemplate", productAPIsUri, productAPIDependsOn.ToArray()));
            }

            // apiTags
            if (apiTagsTemplate != null && apiTagsTemplate.Resources.Count() != 0)
            {
                string apiTagsUri = this.GenerateLinkedTemplateUri(extractorParameters.LinkedTemplatesUrlQueryString, extractorParameters.LinkedTemplatesSasToken, fileNames.TagApi);
                resources.Add(this.CreateLinkedMasterTemplateResource("apiTagsTemplate", apiTagsUri, apiTagDependsOn.ToArray()));
            }
            Console.WriteLine("Master template generated");
            masterTemplate.Resources = resources.ToArray();
            return masterTemplate;
        }

        public MasterTemplateResource CreateLinkedMasterTemplateResourceForApiTemplate(string name, string uriLink, string[] dependsOn, ExtractorParameters extractorParameters)
        {
            MasterTemplateResource masterResourceTemplate = this.CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);
            if (extractorParameters.PolicyXMLBaseUrl != null)
            {
                masterResourceTemplate.Properties.parameters.Add(ParameterNames.PolicyXMLBaseUrl, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.PolicyXMLBaseUrl}')]" });
            }
            if (extractorParameters.PolicyXMLSasToken != null)
            {
                masterResourceTemplate.Properties.parameters.Add(ParameterNames.PolicyXMLSasToken, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.PolicyXMLSasToken}')]" });
            }
            if (extractorParameters.ParameterizeServiceUrl)
            {
                masterResourceTemplate.Properties.parameters.Add(ParameterNames.ServiceUrl, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.ServiceUrl}')]" });
            }
            if (extractorParameters.ParameterizeApiLoggerId)
            {
                masterResourceTemplate.Properties.parameters.Add(ParameterNames.ApiLoggerId, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.ApiLoggerId}')]" });
            }
            return masterResourceTemplate;
        }

        public MasterTemplateResource CreateLinkedMasterTemplateResourceForPropertyTemplate(string name, string uriLink, string[] dependsOn, ExtractorParameters extractorParameters)
        {
            if (extractorParameters is null)
            {
                throw new ArgumentNullException(nameof(extractorParameters));
            }

            MasterTemplateResource masterResourceTemplate = this.CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);
            if (extractorParameters.ParameterizeNamedValue)
            {
                masterResourceTemplate.Properties.parameters.Add(ParameterNames.NamedValues, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.NamedValues}')]" });
            }
            if (extractorParameters.ParamNamedValuesKeyVaultSecrets)
            {
                masterResourceTemplate.Properties.parameters.Add(ParameterNames.NamedValueKeyVaultSecrets, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.NamedValueKeyVaultSecrets}')]" });
            }
            return masterResourceTemplate;
        }

        public MasterTemplateResource CreateLinkedMasterTemplateResourceWithPolicyToken(string name, string uriLink, string[] dependsOn, ExtractorParameters extractorParameters)
        {
            MasterTemplateResource masterResourceTemplate = this.CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);

            if (extractorParameters.PolicyXMLBaseUrl != null)
            {
                masterResourceTemplate.Properties.parameters.Add(ParameterNames.PolicyXMLBaseUrl, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.PolicyXMLBaseUrl}')]" });
            }
            if (extractorParameters.PolicyXMLSasToken != null)
            {
                masterResourceTemplate.Properties.parameters.Add(ParameterNames.PolicyXMLSasToken, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.PolicyXMLSasToken}')]" });
            }
            return masterResourceTemplate;
        }


        public MasterTemplateResource CreateLinkedMasterTemplateResourceForLoggerTemplate(string name, string uriLink, string[] dependsOn, ExtractorParameters extractorParameters)
        {
            MasterTemplateResource masterResourceTemplate = this.CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);
            if (extractorParameters.ParameterizeLogResourceId)
            {
                masterResourceTemplate.Properties.parameters.Add(ParameterNames.LoggerResourceId, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.LoggerResourceId}')]" });
            }
            return masterResourceTemplate;
        }

        public MasterTemplateResource CreateLinkedMasterTemplateResource(string name, string uriLink, string[] dependsOn)
        {
            // create deployment resource with provided arguments
            MasterTemplateResource masterTemplateResource = new MasterTemplateResource()
            {
                Name = name,
                Type = "Microsoft.Resources/deployments",
                ApiVersion = GlobalConstants.ArmApiVersion,
                Properties = new MasterTemplateProperties()
                {
                    mode = "Incremental",
                    templateLink = new MasterTemplateLink()
                    {
                        uri = uriLink,
                        contentVersion = "1.0.0.0"
                    },
                    parameters = new Dictionary<string, TemplateParameterProperties>
                    {
                        { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ Value = $"[parameters('{ParameterNames.ApimServiceName}')]" } }
                    }
                },
                DependsOn = dependsOn
            };
            return masterTemplateResource;
        }

        public Dictionary<string, TemplateParameterProperties> CreateMasterTemplateParameters(bool linked, ExtractorParameters extractorParameters)
        {
            // used to create the parameter metatadata, etc (not value) for use in file with resources
            // add parameters with metatdata properties
            Dictionary<string, TemplateParameterProperties> parameters = new Dictionary<string, TemplateParameterProperties>();
            TemplateParameterProperties apimServiceNameProperties = new TemplateParameterProperties()
            {
                Metadata = new TemplateParameterMetadata()
                {
                    description = "Name of the API Management"
                },
                Type = "string"
            };
            parameters.Add(ParameterNames.ApimServiceName, apimServiceNameProperties);
            // add remote location of template files for linked option
            if (linked == true)
            {
                TemplateParameterProperties linkedTemplatesBaseUrlProperties = new TemplateParameterProperties()
                {
                    Metadata = new TemplateParameterMetadata()
                    {
                        description = "Base URL of the repository that contains the generated templates"
                    },
                    Type = "string"
                };
                parameters.Add(ParameterNames.LinkedTemplatesBaseUrl, linkedTemplatesBaseUrlProperties);
                // add linkedTemplatesSasToken parameter if provided and if the templates are linked
                if (extractorParameters.LinkedTemplatesSasToken != null)
                {
                    TemplateParameterProperties linkedTemplatesSasTokenProperties = new TemplateParameterProperties()
                    {
                        Metadata = new TemplateParameterMetadata()
                        {
                            description = "The Shared Access Signature for the URL of the repository"
                        },
                        Type = "string"
                    };
                    parameters.Add(ParameterNames.LinkedTemplatesSasToken, linkedTemplatesSasTokenProperties);
                }
                // add linkedTemplatesUrlQueryString parameter if provided and if the templates are linked
                if (extractorParameters.LinkedTemplatesUrlQueryString != null)
                {
                    TemplateParameterProperties linkedTemplatesUrlQueryStringProperties = new TemplateParameterProperties()
                    {
                        Metadata = new TemplateParameterMetadata()
                        {
                            description = "Query string for the URL of the repository"
                        },
                        Type = "string"
                    };
                    parameters.Add(ParameterNames.LinkedTemplatesUrlQueryString, linkedTemplatesUrlQueryStringProperties);
                }
            }
            if (extractorParameters.PolicyXMLBaseUrl != null)
            {
                TemplateParameterProperties policyTemplateBaseUrlProperties = new TemplateParameterProperties()
                {
                    Metadata = new TemplateParameterMetadata()
                    {
                        description = "Base URL of the repository that contains the generated policy files"
                    },
                    Type = "string"
                };
                parameters.Add(ParameterNames.PolicyXMLBaseUrl, policyTemplateBaseUrlProperties);
                if (extractorParameters.PolicyXMLSasToken != null)
                {
                    TemplateParameterProperties policyXMLSasTokenProperties = new TemplateParameterProperties()
                    {
                        Metadata = new TemplateParameterMetadata()
                        {
                            description = "The SAS token for the URL of the policy container"
                        },
                        Type = "string"
                    };
                    parameters.Add(ParameterNames.PolicyXMLSasToken, policyXMLSasTokenProperties);
                }
            }
            if (extractorParameters.ParameterizeServiceUrl)
            {
                TemplateParameterProperties paramServiceUrlProperties = new TemplateParameterProperties()
                {
                    Metadata = new TemplateParameterMetadata()
                    {
                        description = "Service url for each Api"
                    },
                    Type = "object"
                };
                parameters.Add(ParameterNames.ServiceUrl, paramServiceUrlProperties);
            }
            if (extractorParameters.ParameterizeNamedValue)
            {
                TemplateParameterProperties namedValueProperties = new TemplateParameterProperties()
                {
                    Metadata = new TemplateParameterMetadata()
                    {
                        description = "Named values"
                    },
                    Type = "object"
                };
                parameters.Add(ParameterNames.NamedValues, namedValueProperties);
            }
            if (extractorParameters.ParameterizeApiLoggerId)
            {
                TemplateParameterProperties loggerIdProperties = new TemplateParameterProperties()
                {
                    Metadata = new TemplateParameterMetadata()
                    {
                        description = "LoggerId for this api"
                    },
                    Type = "object"
                };
                parameters.Add(ParameterNames.ApiLoggerId, loggerIdProperties);
            }
            if (extractorParameters.ParameterizeLogResourceId)
            {
                TemplateParameterProperties loggerResourceIdProperties = new TemplateParameterProperties()
                {
                    Metadata = new TemplateParameterMetadata()
                    {
                        description = "ResourceId for the logger"
                    },
                    Type = "object"
                };
                parameters.Add(ParameterNames.LoggerResourceId, loggerResourceIdProperties);
            }
            if (extractorParameters.ParamNamedValuesKeyVaultSecrets)
            {
                TemplateParameterProperties namedValueKeyVaultSecretsProperties = new TemplateParameterProperties()
                {
                    Metadata = new TemplateParameterMetadata()
                    {
                        description = "Key Vault Secrets for Named Values"
                    },
                    Type = "object"
                };
                parameters.Add(ParameterNames.NamedValueKeyVaultSecrets, namedValueKeyVaultSecretsProperties);
            }
            if (extractorParameters.ParameterizeBackend)
            {
                TemplateParameterProperties backendSettingsProperties = new TemplateParameterProperties()
                {
                    Metadata = new TemplateParameterMetadata()
                    {
                        description = "The settings for the Backends"
                    },
                    Type = "object"
                };
                parameters.Add(ParameterNames.BackendSettings, backendSettingsProperties);
            }
            return parameters;
        }

        // this function will create master / parameter templates for deploying API revisions
        public Template CreateSingleAPIRevisionsMasterTemplate(List<string> revList, string currentRev, ExtractorParameters extractorParameters, FileNames fileNames)
        {
            // create empty template
            Template masterTemplate = this.templateBuilder.GenerateEmptyTemplate().Build();

            // add parameters
            masterTemplate.Parameters = this.CreateMasterTemplateParameters(true, extractorParameters);

            // add deployment resources that links to all resource files
            List<TemplateResource> resources = new List<TemplateResource>();

            string curRevTemplate = string.Concat(currentRev, "MasterTemplate");
            int masterCnt = 0;

            foreach (string apiName in revList)
            {
                string revMasterPath = string.Concat("/", apiName, fileNames.LinkedMaster);
                string revUri = this.GenerateLinkedTemplateUri(extractorParameters.LinkedTemplatesUrlQueryString, extractorParameters.LinkedTemplatesSasToken, revMasterPath);
                string templatename = string.Concat("masterTemplate", masterCnt++);
                if (!apiName.Equals(currentRev))
                {
                    resources.Add(this.CreateLinkedMasterTemplateResource(templatename, revUri, this.GenerateAPIRevisionDependencies(curRevTemplate)));
                }
                else
                {
                    resources.Add(this.CreateLinkedMasterTemplateResource(templatename, revUri, Array.Empty<string>()));
                }
            }

            masterTemplate.Resources = resources.ToArray();
            return masterTemplate;
        }

        public string[] GenerateAPIRevisionDependencies(string curRevTemplate)
        {
            List<string> revDependsOn = new List<string>();
            revDependsOn.Add($"[resourceId('Microsoft.Resources/deployments', '{curRevTemplate}')]");
            return revDependsOn.ToArray();
        }

        public string GenerateLinkedTemplateUri(string linkedTemplatesUrlQueryString, string linkedTemplatesSasToken, string fileName)
        {
            string linkedTemplateUri = linkedTemplatesSasToken != null ?
            $"parameters('{ParameterNames.LinkedTemplatesBaseUrl}'), '{fileName}', parameters('{ParameterNames.LinkedTemplatesSasToken}')"
            : $"parameters('{ParameterNames.LinkedTemplatesBaseUrl}'), '{fileName}'";
            return linkedTemplatesUrlQueryString != null ? $"[concat({linkedTemplateUri}, parameters('{ParameterNames.LinkedTemplatesUrlQueryString}'))]" : $"[concat({linkedTemplateUri})]";
        }
    }
}
