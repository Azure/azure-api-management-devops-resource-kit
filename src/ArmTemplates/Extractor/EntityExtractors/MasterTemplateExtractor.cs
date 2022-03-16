using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class MasterTemplateExtractor : ApiExtractor, IMasterTemplateExtractor
    {
        readonly ITemplateBuilder templateBuilder;

        public MasterTemplateExtractor(
            ILogger<ApiExtractor> logger,
            ITemplateBuilder templateBuilder,
            IApisClient apisClient,
            IDiagnosticExtractor diagnosticExtractor,
            IApiSchemaExtractor apiSchemaExtractor,
            IPolicyExtractor policyExtractor,
            IProductApisExtractor productApisExtractor,
            ITagExtractor tagExtractor,
            IApiOperationExtractor apiOperationExtractor)
            : base(logger, templateBuilder, apisClient, diagnosticExtractor, apiSchemaExtractor, policyExtractor, productApisExtractor, tagExtractor, apiOperationExtractor)
        {
            this.templateBuilder = templateBuilder;
        }

        public Template GenerateLinkedMasterTemplate(
            Template<ApiTemplateResources> apiTemplate,
            Template globalServicePolicyTemplate,
            Template apiVersionSetTemplate,
            Template productsTemplate,
            Template productAPIsTemplate,
            Template apiTagsTemplate,
            Template loggersTemplate,
            Template backendsTemplate,
            Template authorizationServersTemplate,
            Template namedValuesTemplate,
            Template tagTemplate,
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
            string[] dependsOnNamedValues = new string[] { };

            // api dependsOn
            List<string> apiDependsOn = new List<string>();
            List<string> productAPIDependsOn = new List<string>();
            List<string> apiTagDependsOn = new List<string>();

            if (namedValuesTemplate != null && namedValuesTemplate.Resources.Count() != 0)
            {
                dependsOnNamedValues = new string[] { $"[resourceId('Microsoft.Resources/deployments', '{namedValueDeploymentResourceName}')]" };
                apiDependsOn.Add($"[resourceId('Microsoft.Resources/deployments', '{namedValueDeploymentResourceName}')]");
                string namedValuesUri = this.GenerateLinkedTemplateUri(extractorParameters.LinkedTemplatesUrlQueryString, extractorParameters.LinkedTemplatesSasToken, fileNames.NamedValues);
                resources.Add(this.CreateLinkedMasterTemplateResourceForPropertyTemplate(namedValueDeploymentResourceName, namedValuesUri, new string[] { }, extractorParameters));
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
                    templateResource.Properties.parameters.Add(ParameterNames.PolicyXMLBaseUrl, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.PolicyXMLBaseUrl}')]" });
                }
                if (extractorParameters.PolicyXMLSasToken != null)
                {
                    templateResource.Properties.parameters.Add(ParameterNames.PolicyXMLSasToken, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.PolicyXMLSasToken}')]" });
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
                        new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.BackendSettings}')]" });
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
                string apisUri = this.GenerateLinkedTemplateUri(extractorParameters.LinkedTemplatesUrlQueryString, extractorParameters.LinkedTemplatesSasToken, apiTemplate.SpecificResources.FileName);
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
                string apiTagsUri = this.GenerateLinkedTemplateUri(extractorParameters.LinkedTemplatesUrlQueryString, extractorParameters.LinkedTemplatesSasToken, fileNames.ApiTags);
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
                masterResourceTemplate.Properties.parameters.Add(ParameterNames.PolicyXMLBaseUrl, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.PolicyXMLBaseUrl}')]" });
            }
            if (extractorParameters.PolicyXMLSasToken != null)
            {
                masterResourceTemplate.Properties.parameters.Add(ParameterNames.PolicyXMLSasToken, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.PolicyXMLSasToken}')]" });
            }
            if (extractorParameters.ParameterizeServiceUrl)
            {
                masterResourceTemplate.Properties.parameters.Add(ParameterNames.ServiceUrl, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.ServiceUrl}')]" });
            }
            if (extractorParameters.ParameterizeApiLoggerId)
            {
                masterResourceTemplate.Properties.parameters.Add(ParameterNames.ApiLoggerId, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.ApiLoggerId}')]" });
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
                masterResourceTemplate.Properties.parameters.Add(ParameterNames.NamedValues, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.NamedValues}')]" });
            }
            if (extractorParameters.ParamNamedValuesKeyVaultSecrets)
            {
                masterResourceTemplate.Properties.parameters.Add(ParameterNames.NamedValueKeyVaultSecrets, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.NamedValueKeyVaultSecrets}')]" });
            }
            return masterResourceTemplate;
        }

        public MasterTemplateResource CreateLinkedMasterTemplateResourceWithPolicyToken(string name, string uriLink, string[] dependsOn, ExtractorParameters extractorParameters)
        {
            MasterTemplateResource masterResourceTemplate = this.CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);

            if (extractorParameters.PolicyXMLBaseUrl != null)
            {
                masterResourceTemplate.Properties.parameters.Add(ParameterNames.PolicyXMLBaseUrl, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.PolicyXMLBaseUrl}')]" });
            }
            if (extractorParameters.PolicyXMLSasToken != null)
            {
                masterResourceTemplate.Properties.parameters.Add(ParameterNames.PolicyXMLSasToken, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.PolicyXMLSasToken}')]" });
            }
            return masterResourceTemplate;
        }


        public MasterTemplateResource CreateLinkedMasterTemplateResourceForLoggerTemplate(string name, string uriLink, string[] dependsOn, ExtractorParameters extractorParameters)
        {
            MasterTemplateResource masterResourceTemplate = this.CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);
            if (extractorParameters.ParameterizeLogResourceId)
            {
                masterResourceTemplate.Properties.parameters.Add(ParameterNames.LoggerResourceId, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.LoggerResourceId}')]" });
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
                ApiVersion = GlobalConstants.LinkedAPIVersion,
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
                        { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ value = $"[parameters('{ParameterNames.ApimServiceName}')]" } }
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
                metadata = new TemplateParameterMetadata()
                {
                    description = "Name of the API Management"
                },
                type = "string"
            };
            parameters.Add(ParameterNames.ApimServiceName, apimServiceNameProperties);
            // add remote location of template files for linked option
            if (linked == true)
            {
                TemplateParameterProperties linkedTemplatesBaseUrlProperties = new TemplateParameterProperties()
                {
                    metadata = new TemplateParameterMetadata()
                    {
                        description = "Base URL of the repository that contains the generated templates"
                    },
                    type = "string"
                };
                parameters.Add(ParameterNames.LinkedTemplatesBaseUrl, linkedTemplatesBaseUrlProperties);
                // add linkedTemplatesSasToken parameter if provided and if the templates are linked
                if (extractorParameters.LinkedTemplatesSasToken != null)
                {
                    TemplateParameterProperties linkedTemplatesSasTokenProperties = new TemplateParameterProperties()
                    {
                        metadata = new TemplateParameterMetadata()
                        {
                            description = "The Shared Access Signature for the URL of the repository"
                        },
                        type = "string"
                    };
                    parameters.Add(ParameterNames.LinkedTemplatesSasToken, linkedTemplatesSasTokenProperties);
                }
                // add linkedTemplatesUrlQueryString parameter if provided and if the templates are linked
                if (extractorParameters.LinkedTemplatesUrlQueryString != null)
                {
                    TemplateParameterProperties linkedTemplatesUrlQueryStringProperties = new TemplateParameterProperties()
                    {
                        metadata = new TemplateParameterMetadata()
                        {
                            description = "Query string for the URL of the repository"
                        },
                        type = "string"
                    };
                    parameters.Add(ParameterNames.LinkedTemplatesUrlQueryString, linkedTemplatesUrlQueryStringProperties);
                }
            }
            if (extractorParameters.PolicyXMLBaseUrl != null)
            {
                TemplateParameterProperties policyTemplateBaseUrlProperties = new TemplateParameterProperties()
                {
                    metadata = new TemplateParameterMetadata()
                    {
                        description = "Base URL of the repository that contains the generated policy files"
                    },
                    type = "string"
                };
                parameters.Add(ParameterNames.PolicyXMLBaseUrl, policyTemplateBaseUrlProperties);
                if (extractorParameters.PolicyXMLSasToken != null)
                {
                    TemplateParameterProperties policyXMLSasTokenProperties = new TemplateParameterProperties()
                    {
                        metadata = new TemplateParameterMetadata()
                        {
                            description = "The SAS token for the URL of the policy container"
                        },
                        type = "string"
                    };
                    parameters.Add(ParameterNames.PolicyXMLSasToken, policyXMLSasTokenProperties);
                }
            }
            if (extractorParameters.ParameterizeServiceUrl)
            {
                TemplateParameterProperties paramServiceUrlProperties = new TemplateParameterProperties()
                {
                    metadata = new TemplateParameterMetadata()
                    {
                        description = "Service url for each Api"
                    },
                    type = "object"
                };
                parameters.Add(ParameterNames.ServiceUrl, paramServiceUrlProperties);
            }
            if (extractorParameters.ParameterizeNamedValue)
            {
                TemplateParameterProperties namedValueProperties = new TemplateParameterProperties()
                {
                    metadata = new TemplateParameterMetadata()
                    {
                        description = "Named values"
                    },
                    type = "object"
                };
                parameters.Add(ParameterNames.NamedValues, namedValueProperties);
            }
            if (extractorParameters.ParameterizeApiLoggerId)
            {
                TemplateParameterProperties loggerIdProperties = new TemplateParameterProperties()
                {
                    metadata = new TemplateParameterMetadata()
                    {
                        description = "LoggerId for this api"
                    },
                    type = "object"
                };
                parameters.Add(ParameterNames.ApiLoggerId, loggerIdProperties);
            }
            if (extractorParameters.ParameterizeLogResourceId)
            {
                TemplateParameterProperties loggerResourceIdProperties = new TemplateParameterProperties()
                {
                    metadata = new TemplateParameterMetadata()
                    {
                        description = "ResourceId for the logger"
                    },
                    type = "object"
                };
                parameters.Add(ParameterNames.LoggerResourceId, loggerResourceIdProperties);
            }
            if (extractorParameters.ParamNamedValuesKeyVaultSecrets)
            {
                TemplateParameterProperties namedValueKeyVaultSecretsProperties = new TemplateParameterProperties()
                {
                    metadata = new TemplateParameterMetadata()
                    {
                        description = "Key Vault Secrets for Named Values"
                    },
                    type = "object"
                };
                parameters.Add(ParameterNames.NamedValueKeyVaultSecrets, namedValueKeyVaultSecretsProperties);
            }
            if (extractorParameters.ParameterizeBackend)
            {
                TemplateParameterProperties backendSettingsProperties = new TemplateParameterProperties()
                {
                    metadata = new TemplateParameterMetadata()
                    {
                        description = "The settings for the Backends"
                    },
                    type = "object"
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
                    resources.Add(this.CreateLinkedMasterTemplateResource(templatename, revUri, new string[] { }));
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

        public async Task<Template> CreateMasterTemplateParameterValues(List<string> apisToExtract, ExtractorParameters extractorParameters,
            Dictionary<string, object> apiLoggerId,
            Dictionary<string, string> loggerResourceIds,
            Dictionary<string, BackendApiParameters> backendParams,
             List<TemplateResource> propertyResources)
        {
            // used to create the parameter values for use in parameters file
            // create empty template
            Template masterTemplate = this.templateBuilder.GenerateEmptyTemplate().Build();

            // add parameters with value property
            Dictionary<string, TemplateParameterProperties> parameters = new Dictionary<string, TemplateParameterProperties>();
            TemplateParameterProperties apimServiceNameProperties = new TemplateParameterProperties()
            {
                value = extractorParameters.DestinationApimName
            };
            parameters.Add(ParameterNames.ApimServiceName, apimServiceNameProperties);
            if (extractorParameters.LinkedTemplatesBaseUrl != null)
            {
                TemplateParameterProperties linkedTemplatesBaseUrlProperties = new TemplateParameterProperties()
                {
                    value = extractorParameters.LinkedTemplatesBaseUrl
                };
                parameters.Add(ParameterNames.LinkedTemplatesBaseUrl, linkedTemplatesBaseUrlProperties);
                // add linkedTemplatesSasToken parameter if provided and if the user has provided a linkedTemplatesBaseUrl
                if (extractorParameters.LinkedTemplatesSasToken != null)
                {
                    TemplateParameterProperties linkedTemplatesSasTokenProperties = new TemplateParameterProperties()
                    {
                        value = extractorParameters.LinkedTemplatesSasToken
                    };
                    parameters.Add(ParameterNames.LinkedTemplatesSasToken, linkedTemplatesSasTokenProperties);
                }
                // add linkedTemplatesUrlQueryString parameter if provided and if the user has provided a linkedTemplatesBaseUrl
                if (extractorParameters.LinkedTemplatesUrlQueryString != null)
                {
                    TemplateParameterProperties linkedTemplatesUrlQueryStringProperties = new TemplateParameterProperties()
                    {
                        value = extractorParameters.LinkedTemplatesUrlQueryString
                    };
                    parameters.Add(ParameterNames.LinkedTemplatesUrlQueryString, linkedTemplatesUrlQueryStringProperties);
                }
            }
            if (extractorParameters.PolicyXMLBaseUrl != null)
            {
                TemplateParameterProperties policyTemplateBaseUrlProperties = new TemplateParameterProperties()
                {
                    value = extractorParameters.PolicyXMLBaseUrl
                };
                parameters.Add(ParameterNames.PolicyXMLBaseUrl, policyTemplateBaseUrlProperties);
                // add policyXMLSasToken parameter if provided and if the user has provided a policyXMLBaseUrl
                if (extractorParameters.PolicyXMLSasToken != null)
                {
                    TemplateParameterProperties policyTemplateSasTokenProperties = new TemplateParameterProperties()
                    {
                        value = extractorParameters.PolicyXMLSasToken
                    };
                    parameters.Add(ParameterNames.PolicyXMLSasToken, policyTemplateSasTokenProperties);
                }
            }
            if (extractorParameters.ParameterizeServiceUrl)
            {
                Dictionary<string, string> serviceUrls = new Dictionary<string, string>();
                foreach (string apiName in apisToExtract)
                {
                    string validApiName = ParameterNamingHelper.GenerateValidParameterName(apiName, ParameterPrefix.Api);
                    string serviceUrl = extractorParameters.ServiceUrlParameters != null ? this.GetApiServiceUrlFromParameters(apiName, extractorParameters.ServiceUrlParameters) :
                    await this.GetAPIServiceUrl(extractorParameters.SourceApimName, extractorParameters.ResourceGroup, apiName);
                    serviceUrls.Add(validApiName, serviceUrl);
                }
                TemplateObjectParameterProperties serviceUrlProperties = new TemplateObjectParameterProperties()
                {
                    value = serviceUrls
                };
                parameters.Add(ParameterNames.ServiceUrl, serviceUrlProperties);
            }
            if (extractorParameters.ParameterizeNamedValue)
            {
                Dictionary<string, string> namedValues = new Dictionary<string, string>();
                PropertyExtractor pExc = new PropertyExtractor(this.templateBuilder);
                string[] properties = await pExc.GetPropertiesAsync(extractorParameters.SourceApimName, extractorParameters.ResourceGroup);

                foreach (var extractedProperty in properties)
                {
                    JToken oProperty = JObject.Parse(extractedProperty);
                    string propertyName = ((JValue)oProperty["name"]).Value.ToString();

                    // check if the property has been extracted as it is being used in a policy or backend
                    if (propertyResources.Count(item => item.Name.Contains(propertyName)) > 0)
                    {
                        string fullPropertyResource = await pExc.GetPropertyDetailsAsync(extractorParameters.SourceApimName, extractorParameters.ResourceGroup, propertyName);
                        PropertyTemplateResource propertyTemplateResource = JsonConvert.DeserializeObject<PropertyTemplateResource>(fullPropertyResource);

                        //Only add the property if it is not controlled by keyvault
                        if (propertyTemplateResource?.Properties.keyVault == null)
                        {
                            string propertyValue = propertyTemplateResource.Properties.value;
                            string validPName = ParameterNamingHelper.GenerateValidParameterName(propertyName, ParameterPrefix.Property);
                            namedValues.Add(validPName, propertyValue);
                        }
                    }
                }
                TemplateObjectParameterProperties namedValueProperties = new TemplateObjectParameterProperties()
                {
                    value = namedValues
                };
                parameters.Add(ParameterNames.NamedValues, namedValueProperties);
            }
            if (extractorParameters.ParamNamedValuesKeyVaultSecrets)
            {
                Dictionary<string, string> keyVaultNamedValues = new Dictionary<string, string>();
                PropertyExtractor pExc = new PropertyExtractor(this.templateBuilder);
                string[] properties = await pExc.GetPropertiesAsync(extractorParameters.SourceApimName, extractorParameters.ResourceGroup);

                foreach (var extractedProperty in properties)
                {
                    JToken oProperty = JObject.Parse(extractedProperty);
                    string propertyName = ((JValue)oProperty["name"]).Value.ToString();

                    // check if the property has been extracted as it is being used in a policy or backend
                    if (propertyResources.Count(item => item.Name.Contains(propertyName)) > 0)
                    {
                        string fullPropertyResource = await pExc.GetPropertyDetailsAsync(extractorParameters.SourceApimName, extractorParameters.ResourceGroup, propertyName);
                        PropertyTemplateResource propertyTemplateResource = JsonConvert.DeserializeObject<PropertyTemplateResource>(fullPropertyResource);
                        if (propertyTemplateResource?.Properties.keyVault != null)
                        {
                            string propertyValue = propertyTemplateResource.Properties.keyVault.secretIdentifier;
                            string validPName = ParameterNamingHelper.GenerateValidParameterName(propertyName, ParameterPrefix.Property);
                            keyVaultNamedValues.Add(validPName, propertyValue);
                        }
                    }
                }
                TemplateObjectParameterProperties keyVaultNamedValueProperties = new TemplateObjectParameterProperties()
                {
                    value = keyVaultNamedValues
                };
                parameters.Add(ParameterNames.NamedValueKeyVaultSecrets, keyVaultNamedValueProperties);
            }
            if (extractorParameters.ParameterizeApiLoggerId)
            {
                TemplateObjectParameterProperties loggerIdProperties = new TemplateObjectParameterProperties()
                {
                    value = apiLoggerId
                };
                parameters.Add(ParameterNames.ApiLoggerId, loggerIdProperties);
            }
            if (extractorParameters.ParameterizeLogResourceId)
            {
                TemplateObjectParameterProperties loggerResourceIdProperties = new TemplateObjectParameterProperties()
                {
                    value = loggerResourceIds
                };
                parameters.Add(ParameterNames.LoggerResourceId, loggerResourceIdProperties);
            }

            if (extractorParameters.ParameterizeBackend)
            {
                TemplateObjectParameterProperties backendProperties = new TemplateObjectParameterProperties()
                {
                    value = backendParams
                };
                parameters.Add(ParameterNames.BackendSettings, backendProperties);
            }
            masterTemplate.Parameters = parameters;
            return masterTemplate;
        }

        public string GetApiServiceUrlFromParameters(string apiName, ServiceUrlProperty[] serviceUrlParameters)
        {
            foreach (ServiceUrlProperty ele in serviceUrlParameters)
            {
                if (ele.ApiName.Equals(apiName))
                {
                    return ele.ServiceUrl;
                }
            }
            return null;
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
