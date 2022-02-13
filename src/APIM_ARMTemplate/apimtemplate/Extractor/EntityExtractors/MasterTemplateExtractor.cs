using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using apimtemplate.Common.TemplateModels;
using apimtemplate.Extractor.EntityExtractors.Abstractions;
using apimtemplate.Extractor.Models;
using apimtemplate.Common.FileHandlers;
using apimtemplate.Common.Helpers;
using apimtemplate.Common.Templates.Abstractions;
using apimtemplate.Common.Constants;

namespace apimtemplate.Extractor.EntityExtractors
{
    public class MasterTemplateExtractor : ApiExtractor, IMasterTemplateExtractor
    {
        public Template GenerateLinkedMasterTemplate(Template apiTemplate,
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
            string apiFileName,
            ExtractorParameters extractorParameters)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Generating master template");
            // create empty template
            Template masterTemplate = GenerateEmptyTemplate();

            // add parameters
            masterTemplate.parameters = CreateMasterTemplateParameters(true, extractorParameters);

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

            if (namedValuesTemplate != null && namedValuesTemplate.resources.Count() != 0)
            {
                dependsOnNamedValues = new string[] { $"[resourceId('Microsoft.Resources/deployments', '{namedValueDeploymentResourceName}')]" };
                apiDependsOn.Add($"[resourceId('Microsoft.Resources/deployments', '{namedValueDeploymentResourceName}')]");
                string namedValuesUri = GenerateLinkedTemplateUri(extractorParameters.linkedTemplatesUrlQueryString, extractorParameters.linkedTemplatesSasToken, fileNames.namedValues);
                resources.Add(CreateLinkedMasterTemplateResourceForPropertyTemplate(namedValueDeploymentResourceName, namedValuesUri, new string[] { }, extractorParameters));
            }

            // globalServicePolicy
            if (globalServicePolicyTemplate != null && globalServicePolicyTemplate.resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'globalServicePolicyTemplate')]");
                string globalServicePolicyUri = GenerateLinkedTemplateUri(extractorParameters.linkedTemplatesUrlQueryString, extractorParameters.linkedTemplatesSasToken, fileNames.globalServicePolicy);
                resources.Add(CreateLinkedMasterTemplateResourceWithPolicyToken("globalServicePolicyTemplate", globalServicePolicyUri, dependsOnNamedValues, extractorParameters));
            }

            // apiVersionSet
            if (apiVersionSetTemplate != null && apiVersionSetTemplate.resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'versionSetTemplate')]");
                string apiVersionSetUri = GenerateLinkedTemplateUri(extractorParameters.linkedTemplatesUrlQueryString, extractorParameters.linkedTemplatesSasToken, fileNames.apiVersionSets);
                resources.Add(CreateLinkedMasterTemplateResource("versionSetTemplate", apiVersionSetUri, dependsOnNamedValues));
            }

            // product
            if (productsTemplate != null && productsTemplate.resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'productsTemplate')]");
                productAPIDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'productsTemplate')]");
                string productsUri = GenerateLinkedTemplateUri(extractorParameters.linkedTemplatesUrlQueryString, extractorParameters.linkedTemplatesSasToken, fileNames.products);
                var templateResource = CreateLinkedMasterTemplateResource("productsTemplate", productsUri, dependsOnNamedValues);
                if (extractorParameters.policyXMLBaseUrl != null)
                {
                    templateResource.properties.parameters.Add(ParameterNames.PolicyXMLBaseUrl, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.PolicyXMLBaseUrl}')]" });
                }
                if (extractorParameters.policyXMLSasToken != null)
                {
                    templateResource.properties.parameters.Add(ParameterNames.PolicyXMLSasToken, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.PolicyXMLSasToken}')]" });
                }
                resources.Add(templateResource);
            }

            if (tagTemplate != null && tagTemplate.resources.Count() != 0)
            {
                apiTagDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'tagTemplate')]");
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'tagTemplate')]");
                string tagUri = GenerateLinkedTemplateUri(extractorParameters.linkedTemplatesUrlQueryString, extractorParameters.linkedTemplatesSasToken, fileNames.tags);
                resources.Add(CreateLinkedMasterTemplateResource("tagTemplate", tagUri, dependsOnNamedValues));
            }

            // logger
            if (loggersTemplate != null && loggersTemplate.resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'loggersTemplate')]");
                string loggersUri = GenerateLinkedTemplateUri(extractorParameters.linkedTemplatesUrlQueryString, extractorParameters.linkedTemplatesSasToken, fileNames.loggers);
                resources.Add(CreateLinkedMasterTemplateResourceForLoggerTemplate("loggersTemplate", loggersUri, dependsOnNamedValues, extractorParameters));
            }

            // backend
            if (backendsTemplate != null && backendsTemplate.resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'backendsTemplate')]");
                string backendsUri = GenerateLinkedTemplateUri(extractorParameters.linkedTemplatesUrlQueryString, extractorParameters.linkedTemplatesSasToken, fileNames.backends);
                var templateResource = CreateLinkedMasterTemplateResource("backendsTemplate", backendsUri, dependsOnNamedValues);
                if (extractorParameters.paramBackend)
                {
                    templateResource.properties.parameters.Add(ParameterNames.BackendSettings,
                        new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.BackendSettings}')]" });
                }

                resources.Add(templateResource);
            }

            // authorizationServer
            if (authorizationServersTemplate != null && authorizationServersTemplate.resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'authorizationServersTemplate')]");
                string authorizationServersUri = GenerateLinkedTemplateUri(extractorParameters.linkedTemplatesUrlQueryString, extractorParameters.linkedTemplatesSasToken, fileNames.authorizationServers);
                resources.Add(CreateLinkedMasterTemplateResource("authorizationServersTemplate", authorizationServersUri, dependsOnNamedValues));
            }

            // api
            if (apiTemplate != null && apiTemplate.resources.Count() != 0)
            {
                apiTagDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'apisTemplate')]");
                productAPIDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'apisTemplate')]");
                string apisUri = GenerateLinkedTemplateUri(extractorParameters.linkedTemplatesUrlQueryString, extractorParameters.linkedTemplatesSasToken, apiFileName);
                resources.Add(CreateLinkedMasterTemplateResourceForApiTemplate("apisTemplate", apisUri, apiDependsOn.ToArray(), extractorParameters));
            }

            // productAPIs
            if (productAPIsTemplate != null && productAPIsTemplate.resources.Count() != 0)
            {
                string productAPIsUri = GenerateLinkedTemplateUri(extractorParameters.linkedTemplatesUrlQueryString, extractorParameters.linkedTemplatesSasToken, fileNames.productAPIs);

                resources.Add(CreateLinkedMasterTemplateResource("productAPIsTemplate", productAPIsUri, productAPIDependsOn.ToArray()));
            }

            // apiTags
            if (apiTagsTemplate != null && apiTagsTemplate.resources.Count() != 0)
            {
                string apiTagsUri = GenerateLinkedTemplateUri(extractorParameters.linkedTemplatesUrlQueryString, extractorParameters.linkedTemplatesSasToken, fileNames.apiTags);
                resources.Add(CreateLinkedMasterTemplateResource("apiTagsTemplate", apiTagsUri, apiTagDependsOn.ToArray()));
            }
            Console.WriteLine("Master template generated");
            masterTemplate.resources = resources.ToArray();
            return masterTemplate;
        }

        public MasterTemplateResource CreateLinkedMasterTemplateResourceForApiTemplate(string name, string uriLink, string[] dependsOn, ExtractorParameters extractorParameters)
        {
            MasterTemplateResource masterResourceTemplate = CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);
            if (extractorParameters.policyXMLBaseUrl != null)
            {
                masterResourceTemplate.properties.parameters.Add(ParameterNames.PolicyXMLBaseUrl, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.PolicyXMLBaseUrl}')]" });
            }
            if (extractorParameters.policyXMLSasToken != null)
            {
                masterResourceTemplate.properties.parameters.Add(ParameterNames.PolicyXMLSasToken, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.PolicyXMLSasToken}')]" });
            }
            if (extractorParameters.paramServiceUrl)
            {
                masterResourceTemplate.properties.parameters.Add(ParameterNames.ServiceUrl, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.ServiceUrl}')]" });
            }
            if (extractorParameters.paramApiLoggerId)
            {
                masterResourceTemplate.properties.parameters.Add(ParameterNames.ApiLoggerId, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.ApiLoggerId}')]" });
            }
            return masterResourceTemplate;
        }

        public MasterTemplateResource CreateLinkedMasterTemplateResourceForPropertyTemplate(string name, string uriLink, string[] dependsOn, ExtractorParameters extractorParameters)
        {
            if (extractorParameters is null)
            {
                throw new ArgumentNullException(nameof(extractorParameters));
            }

            MasterTemplateResource masterResourceTemplate = CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);
            if (extractorParameters.paramNamedValue)
            {
                masterResourceTemplate.properties.parameters.Add(ParameterNames.NamedValues, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.NamedValues}')]" });
            }
            if (extractorParameters.paramNamedValuesKeyVaultSecrets)
            {
                masterResourceTemplate.properties.parameters.Add(ParameterNames.NamedValueKeyVaultSecrets, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.NamedValueKeyVaultSecrets}')]" });
            }
            return masterResourceTemplate;
        }

        public MasterTemplateResource CreateLinkedMasterTemplateResourceWithPolicyToken(string name, string uriLink, string[] dependsOn, ExtractorParameters extractorParameters)
        {
            MasterTemplateResource masterResourceTemplate = CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);

            if (extractorParameters.policyXMLBaseUrl != null)
            {
                masterResourceTemplate.properties.parameters.Add(ParameterNames.PolicyXMLBaseUrl, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.PolicyXMLBaseUrl}')]" });
            }
            if (extractorParameters.policyXMLSasToken != null)
            {
                masterResourceTemplate.properties.parameters.Add(ParameterNames.PolicyXMLSasToken, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.PolicyXMLSasToken}')]" });
            }
            return masterResourceTemplate;
        }


        public MasterTemplateResource CreateLinkedMasterTemplateResourceForLoggerTemplate(string name, string uriLink, string[] dependsOn, ExtractorParameters extractorParameters)
        {
            MasterTemplateResource masterResourceTemplate = CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);
            if (extractorParameters.paramLogResourceId)
            {
                masterResourceTemplate.properties.parameters.Add(ParameterNames.LoggerResourceId, new TemplateParameterProperties() { value = $"[parameters('{ParameterNames.LoggerResourceId}')]" });
            }
            return masterResourceTemplate;
        }

        public MasterTemplateResource CreateLinkedMasterTemplateResource(string name, string uriLink, string[] dependsOn)
        {
            // create deployment resource with provided arguments
            MasterTemplateResource masterTemplateResource = new MasterTemplateResource()
            {
                name = name,
                type = "Microsoft.Resources/deployments",
                apiVersion = GlobalConstants.LinkedAPIVersion,
                properties = new MasterTemplateProperties()
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
                dependsOn = dependsOn
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
                if (extractorParameters.linkedTemplatesSasToken != null)
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
                if (extractorParameters.linkedTemplatesUrlQueryString != null)
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
            if (extractorParameters.policyXMLBaseUrl != null)
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
                if (extractorParameters.policyXMLSasToken != null)
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
            if (extractorParameters.paramServiceUrl)
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
            if (extractorParameters.paramNamedValue)
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
            if (extractorParameters.paramApiLoggerId)
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
            if (extractorParameters.paramLogResourceId)
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
            if (extractorParameters.paramNamedValuesKeyVaultSecrets)
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
            if (extractorParameters.paramBackend)
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
            Template masterTemplate = GenerateEmptyTemplate();

            // add parameters
            masterTemplate.parameters = CreateMasterTemplateParameters(true, extractorParameters);

            // add deployment resources that links to all resource files
            List<TemplateResource> resources = new List<TemplateResource>();

            string curRevTemplate = string.Concat(currentRev, "MasterTemplate");
            int masterCnt = 0;

            foreach (string apiName in revList)
            {
                string revMasterPath = string.Concat("/", apiName, fileNames.linkedMaster);
                string revUri = GenerateLinkedTemplateUri(extractorParameters.linkedTemplatesUrlQueryString, extractorParameters.linkedTemplatesSasToken, revMasterPath);
                string templatename = string.Concat("masterTemplate", masterCnt++);
                if (!apiName.Equals(currentRev))
                {
                    resources.Add(CreateLinkedMasterTemplateResource(templatename, revUri, GenerateAPIRevisionDependencies(curRevTemplate)));
                }
                else
                {
                    resources.Add(CreateLinkedMasterTemplateResource(templatename, revUri, new string[] { }));
                }
            }

            masterTemplate.resources = resources.ToArray();
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
            Template masterTemplate = GenerateEmptyTemplate();

            // add parameters with value property
            Dictionary<string, TemplateParameterProperties> parameters = new Dictionary<string, TemplateParameterProperties>();
            TemplateParameterProperties apimServiceNameProperties = new TemplateParameterProperties()
            {
                value = extractorParameters.destinationApimName
            };
            parameters.Add(ParameterNames.ApimServiceName, apimServiceNameProperties);
            if (extractorParameters.linkedTemplatesBaseUrl != null)
            {
                TemplateParameterProperties linkedTemplatesBaseUrlProperties = new TemplateParameterProperties()
                {
                    value = extractorParameters.linkedTemplatesBaseUrl
                };
                parameters.Add(ParameterNames.LinkedTemplatesBaseUrl, linkedTemplatesBaseUrlProperties);
                // add linkedTemplatesSasToken parameter if provided and if the user has provided a linkedTemplatesBaseUrl
                if (extractorParameters.linkedTemplatesSasToken != null)
                {
                    TemplateParameterProperties linkedTemplatesSasTokenProperties = new TemplateParameterProperties()
                    {
                        value = extractorParameters.linkedTemplatesSasToken
                    };
                    parameters.Add(ParameterNames.LinkedTemplatesSasToken, linkedTemplatesSasTokenProperties);
                }
                // add linkedTemplatesUrlQueryString parameter if provided and if the user has provided a linkedTemplatesBaseUrl
                if (extractorParameters.linkedTemplatesUrlQueryString != null)
                {
                    TemplateParameterProperties linkedTemplatesUrlQueryStringProperties = new TemplateParameterProperties()
                    {
                        value = extractorParameters.linkedTemplatesUrlQueryString
                    };
                    parameters.Add(ParameterNames.LinkedTemplatesUrlQueryString, linkedTemplatesUrlQueryStringProperties);
                }
            }
            if (extractorParameters.policyXMLBaseUrl != null)
            {
                TemplateParameterProperties policyTemplateBaseUrlProperties = new TemplateParameterProperties()
                {
                    value = extractorParameters.policyXMLBaseUrl
                };
                parameters.Add(ParameterNames.PolicyXMLBaseUrl, policyTemplateBaseUrlProperties);
                // add policyXMLSasToken parameter if provided and if the user has provided a policyXMLBaseUrl
                if (extractorParameters.policyXMLSasToken != null)
                {
                    TemplateParameterProperties policyTemplateSasTokenProperties = new TemplateParameterProperties()
                    {
                        value = extractorParameters.policyXMLSasToken
                    };
                    parameters.Add(ParameterNames.PolicyXMLSasToken, policyTemplateSasTokenProperties);
                }
            }
            if (extractorParameters.paramServiceUrl)
            {
                Dictionary<string, string> serviceUrls = new Dictionary<string, string>();
                foreach (string apiName in apisToExtract)
                {
                    string validApiName = ParameterNamingHelper.GenerateValidParameterName(apiName, ParameterPrefix.Api);
                    string serviceUrl = extractorParameters.serviceUrlParameters != null ? GetApiServiceUrlFromParameters(apiName, extractorParameters.serviceUrlParameters) :
                    await GetAPIServiceUrl(extractorParameters.sourceApimName, extractorParameters.resourceGroup, apiName);
                    serviceUrls.Add(validApiName, serviceUrl);
                }
                TemplateObjectParameterProperties serviceUrlProperties = new TemplateObjectParameterProperties()
                {
                    value = serviceUrls
                };
                parameters.Add(ParameterNames.ServiceUrl, serviceUrlProperties);
            }
            if (extractorParameters.paramNamedValue)
            {
                Dictionary<string, string> namedValues = new Dictionary<string, string>();
                PropertyExtractor pExc = new PropertyExtractor();
                string[] properties = await pExc.GetPropertiesAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup);

                foreach (var extractedProperty in properties)
                {
                    JToken oProperty = JObject.Parse(extractedProperty);
                    string propertyName = ((JValue)oProperty["name"]).Value.ToString();

                    // check if the property has been extracted as it is being used in a policy or backend
                    if (propertyResources.Count(item => item.name.Contains(propertyName)) > 0)
                    {
                        string fullPropertyResource = await pExc.GetPropertyDetailsAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup, propertyName);
                        PropertyTemplateResource propertyTemplateResource = JsonConvert.DeserializeObject<PropertyTemplateResource>(fullPropertyResource);

                        //Only add the property if it is not controlled by keyvault
                        if (propertyTemplateResource?.properties.keyVault == null)
                        {
                            string propertyValue = propertyTemplateResource.properties.value;
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
            if (extractorParameters.paramNamedValuesKeyVaultSecrets)
            {
                Dictionary<string, string> keyVaultNamedValues = new Dictionary<string, string>();
                PropertyExtractor pExc = new PropertyExtractor();
                string[] properties = await pExc.GetPropertiesAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup);

                foreach (var extractedProperty in properties)
                {
                    JToken oProperty = JObject.Parse(extractedProperty);
                    string propertyName = ((JValue)oProperty["name"]).Value.ToString();

                    // check if the property has been extracted as it is being used in a policy or backend
                    if (propertyResources.Count(item => item.name.Contains(propertyName)) > 0)
                    {
                        string fullPropertyResource = await pExc.GetPropertyDetailsAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup, propertyName);
                        PropertyTemplateResource propertyTemplateResource = JsonConvert.DeserializeObject<PropertyTemplateResource>(fullPropertyResource);
                        if (propertyTemplateResource?.properties.keyVault != null)
                        {
                            string propertyValue = propertyTemplateResource.properties.keyVault.secretIdentifier;
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
            if (extractorParameters.paramApiLoggerId)
            {
                TemplateObjectParameterProperties loggerIdProperties = new TemplateObjectParameterProperties()
                {
                    value = apiLoggerId
                };
                parameters.Add(ParameterNames.ApiLoggerId, loggerIdProperties);
            }
            if (extractorParameters.paramLogResourceId)
            {
                TemplateObjectParameterProperties loggerResourceIdProperties = new TemplateObjectParameterProperties()
                {
                    value = loggerResourceIds
                };
                parameters.Add(ParameterNames.LoggerResourceId, loggerResourceIdProperties);
            }

            if (extractorParameters.paramBackend)
            {
                TemplateObjectParameterProperties backendProperties = new TemplateObjectParameterProperties()
                {
                    value = backendParams
                };
                parameters.Add(ParameterNames.BackendSettings, backendProperties);
            }
            masterTemplate.parameters = parameters;
            return masterTemplate;
        }

        public string GetApiServiceUrlFromParameters(string apiName, serviceUrlProperty[] serviceUrlParameters)
        {
            foreach (serviceUrlProperty ele in serviceUrlParameters)
            {
                if (ele.apiName.Equals(apiName))
                {
                    return ele.serviceUrl;
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
