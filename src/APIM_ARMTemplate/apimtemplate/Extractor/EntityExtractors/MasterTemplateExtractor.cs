using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Linq;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class MasterTemplateExtractor : EntityExtractor
    {
        public Template GenerateLinkedMasterTemplate(Template apiTemplate,
            Template globalServicePolicyTemplate,
            Template apiVersionSetTemplate,
            Template productsTemplate,
            Template loggersTemplate,
            Template backendsTemplate,
            Template authorizationServersTemplate,
            Template namedValuesTemplate,
            Template tagTemplate,
            FileNames fileNames,
            string apiFileName,
            Extractor exc)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Generating master template");
            // create empty template
            Template masterTemplate = GenerateEmptyTemplate();

            // add parameters
            masterTemplate.parameters = this.CreateMasterTemplateParameters(true, exc);

            // add deployment resources that links to all resource files
            List<TemplateResource> resources = new List<TemplateResource>();

            // namedValue
            string namedValueDeploymentResourceName = "namedValuesTemplate";
            // all other deployment resources will depend on named values
            string[] dependsOnNamedValues = new string[] { };

            // api dependsOn
            List<string> apiDependsOn = new List<string>();

            if (namedValuesTemplate != null && namedValuesTemplate.resources.Count() != 0)
            {
                dependsOnNamedValues = new string[] { $"[resourceId('Microsoft.Resources/deployments', '{namedValueDeploymentResourceName}')]" };
                apiDependsOn.Add($"[resourceId('Microsoft.Resources/deployments', '{namedValueDeploymentResourceName}')]");
                string namedValuesUri = GenerateLinkedTemplateUri(exc.linkedTemplatesUrlQueryString, exc.linkedTemplatesSasToken, fileNames.namedValues);
                resources.Add(this.CreateLinkedMasterTemplateResourceForPropertyTemplate(namedValueDeploymentResourceName, namedValuesUri, new string[] { }, exc));
            }

            // globalServicePolicy
            if (globalServicePolicyTemplate != null && globalServicePolicyTemplate.resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'globalServicePolicyTemplate')]");
                string globalServicePolicyUri = GenerateLinkedTemplateUri(exc.linkedTemplatesUrlQueryString, exc.linkedTemplatesSasToken, fileNames.globalServicePolicy);
                resources.Add(this.CreateLinkedMasterTemplateResourceWithPolicyToken("globalServicePolicyTemplate", globalServicePolicyUri, dependsOnNamedValues, exc));
            }

            // apiVersionSet
            if (apiVersionSetTemplate != null && apiVersionSetTemplate.resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'versionSetTemplate')]");
                string apiVersionSetUri = GenerateLinkedTemplateUri(exc.linkedTemplatesUrlQueryString, exc.linkedTemplatesSasToken, fileNames.apiVersionSets);
                resources.Add(this.CreateLinkedMasterTemplateResourceWithPolicyToken("versionSetTemplate", apiVersionSetUri, dependsOnNamedValues, exc));
            }

            // product
            if (productsTemplate != null && productsTemplate.resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'productsTemplate')]");
                string productsUri = GenerateLinkedTemplateUri(exc.linkedTemplatesUrlQueryString, exc.linkedTemplatesSasToken, fileNames.products);
                resources.Add(this.CreateLinkedMasterTemplateResourceWithPolicyToken("productsTemplate", productsUri, dependsOnNamedValues, exc));
            }

            if (tagTemplate != null && tagTemplate.resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'tagTemplate')]");
                string tagUri = GenerateLinkedTemplateUri(exc.linkedTemplatesUrlQueryString, exc.linkedTemplatesSasToken, fileNames.tags);
                resources.Add(this.CreateLinkedMasterTemplateResourceWithPolicyToken("tagTemplate", tagUri, dependsOnNamedValues, exc));
            }

            // logger
            if (loggersTemplate != null && loggersTemplate.resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'loggersTemplate')]");
                string loggersUri = GenerateLinkedTemplateUri(exc.linkedTemplatesUrlQueryString, exc.linkedTemplatesSasToken, fileNames.loggers);
                resources.Add(this.CreateLinkedMasterTemplateResourceWithPolicyToken("loggersTemplate", loggersUri, dependsOnNamedValues, exc));
            }

            // backend
            if (backendsTemplate != null && backendsTemplate.resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'backendsTemplate')]");
                string backendsUri = GenerateLinkedTemplateUri(exc.linkedTemplatesUrlQueryString, exc.linkedTemplatesSasToken, fileNames.backends);
                resources.Add(this.CreateLinkedMasterTemplateResourceWithPolicyToken("backendsTemplate", backendsUri, dependsOnNamedValues, exc));
            }

            // authorizationServer
            if (authorizationServersTemplate != null && authorizationServersTemplate.resources.Count() != 0)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'authorizationServersTemplate')]");
                string authorizationServersUri = GenerateLinkedTemplateUri(exc.linkedTemplatesUrlQueryString, exc.linkedTemplatesSasToken, fileNames.authorizationServers);
                resources.Add(this.CreateLinkedMasterTemplateResourceWithPolicyToken("authorizationServersTemplate", authorizationServersUri, dependsOnNamedValues, exc));
            }

            // api
            if (apiTemplate != null && apiTemplate.resources.Count() != 0)
            {
                string apisUri = GenerateLinkedTemplateUri(exc.linkedTemplatesUrlQueryString, exc.linkedTemplatesSasToken, apiFileName);
                resources.Add(this.CreateLinkedMasterTemplateResourceForApiTemplate("apisTemplate", apisUri, apiDependsOn.ToArray(), exc));
            }
            Console.WriteLine("Master template generated");
            masterTemplate.resources = resources.ToArray();
            return masterTemplate;
        }

        public MasterTemplateResource CreateLinkedMasterTemplateResourceForApiTemplate(string name, string uriLink, string[] dependsOn, Extractor exc)
        {
            MasterTemplateResource masterResourceTemplate = this.CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);
            if (exc.policyXMLSasToken != null)
            {
                masterResourceTemplate.properties.parameters.Add("PolicyXMLSasToken", new TemplateParameterProperties() { value = "[parameters('PolicyXMLSasToken')]" });
            }
            if (exc.paramServiceUrl)
            {
                masterResourceTemplate.properties.parameters.Add("serviceUrl", new TemplateParameterProperties() { value = "[parameters('serviceUrl')]" });
            }
            if (exc.paramApiLoggerId)
            {
                masterResourceTemplate.properties.parameters.Add("ApiLoggerId", new TemplateParameterProperties() { value = "[parameters('ApiLoggerId')]" });
            }
            return masterResourceTemplate;
        }

        public MasterTemplateResource CreateLinkedMasterTemplateResourceForPropertyTemplate(string name, string uriLink, string[] dependsOn, Extractor exc)
        {
            MasterTemplateResource masterResourceTemplate = this.CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);
            if (exc.policyXMLSasToken != null)
            {
                masterResourceTemplate.properties.parameters.Add("PolicyXMLSasToken", new TemplateParameterProperties() { value = "[parameters('PolicyXMLSasToken')]" });
            }
            if (exc.paramNamedValue)
            {
                masterResourceTemplate.properties.parameters.Add("NamedValues", new TemplateParameterProperties() { value = "[parameters('NamedValues')]" });
            }
            return masterResourceTemplate;
        }

        public MasterTemplateResource CreateLinkedMasterTemplateResourceWithPolicyToken(string name, string uriLink, string[] dependsOn, Extractor exc)
        {
            if (exc.policyXMLSasToken == null)
            {
                return this.CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);
            }
            else
            {
                MasterTemplateResource masterResourceTemplate = this.CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);
                masterResourceTemplate.properties.parameters.Add("PolicyXMLSasToken", new TemplateParameterProperties() { value = "[parameters('PolicyXMLSasToken')]" });
                return masterResourceTemplate;
            }
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
                        { "ApimServiceName", new TemplateParameterProperties(){ value = "[parameters('ApimServiceName')]" } },
                        { "PolicyXMLBaseUrl", new TemplateParameterProperties(){ value = "[parameters('PolicyXMLBaseUrl')]" } }
                    }
                },
                dependsOn = dependsOn
            };
            return masterTemplateResource;
        }

        public Dictionary<string, TemplateParameterProperties> CreateMasterTemplateParameters(bool linked, Extractor exc)
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
            parameters.Add("ApimServiceName", apimServiceNameProperties);
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
                parameters.Add("LinkedTemplatesBaseUrl", linkedTemplatesBaseUrlProperties);
                // add linkedTemplatesSasToken parameter if provided and if the templates are linked
                if (exc.linkedTemplatesSasToken != null)
                {
                    TemplateParameterProperties linkedTemplatesSasTokenProperties = new TemplateParameterProperties()
                    {
                        metadata = new TemplateParameterMetadata()
                        {
                            description = "The Shared Access Signature for the URL of the repository"
                        },
                        type = "string"
                    };
                    parameters.Add("LinkedTemplatesSasToken", linkedTemplatesSasTokenProperties);
                }
                // add linkedTemplatesUrlQueryString parameter if provided and if the templates are linked
                if (exc.linkedTemplatesUrlQueryString != null)
                {
                    TemplateParameterProperties linkedTemplatesUrlQueryStringProperties = new TemplateParameterProperties()
                    {
                        metadata = new TemplateParameterMetadata()
                        {
                            description = "Query string for the URL of the repository"
                        },
                        type = "string"
                    };
                    parameters.Add("LinkedTemplatesUrlQueryString", linkedTemplatesUrlQueryStringProperties);
                }
            }
            if (exc.policyXMLBaseUrl != null)
            {
                TemplateParameterProperties policyTemplateBaseUrlProperties = new TemplateParameterProperties()
                {
                    metadata = new TemplateParameterMetadata()
                    {
                        description = "Base URL of the repository that contains the generated policy files"
                    },
                    type = "string"
                };
                parameters.Add("PolicyXMLBaseUrl", policyTemplateBaseUrlProperties);
                if (exc.policyXMLSasToken != null)
                {
                    TemplateParameterProperties policyXMLSasTokenProperties = new TemplateParameterProperties()
                    {
                        metadata = new TemplateParameterMetadata()
                        {
                            description = "The SAS token for the URL of the policy container"
                        },
                        type = "string"
                    };
                    parameters.Add("PolicyXMLSasToken", policyXMLSasTokenProperties);
                }
            }
            if (exc.paramServiceUrl)
            {
                TemplateParameterProperties paramServiceUrlProperties = new TemplateParameterProperties()
                {
                    metadata = new TemplateParameterMetadata()
                    {
                        description = "Service url for each Api"
                    },
                    type = "object"
                };
                parameters.Add("serviceUrl", paramServiceUrlProperties);
            }
            if (exc.paramNamedValue)
            {
                TemplateParameterProperties namedValueProperties = new TemplateParameterProperties()
                {
                    metadata = new TemplateParameterMetadata()
                    {
                        description = "Named values"
                    },
                    type = "object"
                };
                parameters.Add("NamedValues", namedValueProperties);
            }
            if (exc.paramApiLoggerId)
            {
                TemplateParameterProperties loggerIdProperties = new TemplateParameterProperties()
                {
                    metadata = new TemplateParameterMetadata()
                    {
                        description = "LoggerId for this api"
                    },
                    type = "object"
                };
                parameters.Add("ApiLoggerId", loggerIdProperties);
            }
            return parameters;
        }

        // this function will create master / parameter templates for deploying API revisions
        public Template CreateSingleAPIRevisionsMasterTemplate(List<string> revList, string currentRev, Extractor exc, FileNames fileNames)
        {
            // create empty template
            Template masterTemplate = GenerateEmptyTemplate();

            // add parameters
            masterTemplate.parameters = this.CreateMasterTemplateParameters(true, exc);

            // add deployment resources that links to all resource files
            List<TemplateResource> resources = new List<TemplateResource>();

            string curRevTemplate = String.Concat(currentRev, "MasterTemplate");
            int masterCnt = 0;

            foreach (string apiName in revList)
            {
                string revMasterPath = String.Concat("/", apiName, fileNames.linkedMaster);
                string revUri = GenerateLinkedTemplateUri(exc.linkedTemplatesUrlQueryString, exc.linkedTemplatesSasToken, revMasterPath);
                string templatename = String.Concat("masterTemplate", masterCnt++);
                if (!apiName.Equals(currentRev))
                {
                    resources.Add(this.CreateLinkedMasterTemplateResource(templatename, revUri, GenerateAPIRevisionDependencies(curRevTemplate)));
                }
                else
                {
                    resources.Add(this.CreateLinkedMasterTemplateResource(templatename, revUri, new string[] { }));
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

        public async Task<Template> CreateMasterTemplateParameterValues(List<string> apisToExtract, Extractor exc, Dictionary<string, Dictionary<string, string>> apiLoggerId)
        {
            // used to create the parameter values for use in parameters file
            // create empty template
            Template masterTemplate = GenerateEmptyTemplate();

            // add parameters with value property
            Dictionary<string, TemplateParameterProperties> parameters = new Dictionary<string, TemplateParameterProperties>();
            TemplateParameterProperties apimServiceNameProperties = new TemplateParameterProperties()
            {
                value = exc.destinationApimName
            };
            parameters.Add("ApimServiceName", apimServiceNameProperties);
            if (exc.linkedTemplatesBaseUrl != null)
            {
                TemplateParameterProperties linkedTemplatesBaseUrlProperties = new TemplateParameterProperties()
                {
                    value = exc.linkedTemplatesBaseUrl
                };
                parameters.Add("LinkedTemplatesBaseUrl", linkedTemplatesBaseUrlProperties);
                // add linkedTemplatesSasToken parameter if provided and if the user has provided a linkedTemplatesBaseUrl
                if (exc.linkedTemplatesSasToken != null)
                {
                    TemplateParameterProperties linkedTemplatesSasTokenProperties = new TemplateParameterProperties()
                    {
                        value = exc.linkedTemplatesSasToken
                    };
                    parameters.Add("LinkedTemplatesSasToken", linkedTemplatesSasTokenProperties);
                }
                // add linkedTemplatesUrlQueryString parameter if provided and if the user has provided a linkedTemplatesBaseUrl
                if (exc.linkedTemplatesUrlQueryString != null)
                {
                    TemplateParameterProperties linkedTemplatesUrlQueryStringProperties = new TemplateParameterProperties()
                    {
                        value = exc.linkedTemplatesUrlQueryString
                    };
                    parameters.Add("LinkedTemplatesUrlQueryString", linkedTemplatesUrlQueryStringProperties);
                }
            }
            if (exc.policyXMLBaseUrl != null)
            {
                TemplateParameterProperties policyTemplateBaseUrlProperties = new TemplateParameterProperties()
                {
                    value = exc.policyXMLBaseUrl
                };
                parameters.Add("PolicyXMLBaseUrl", policyTemplateBaseUrlProperties);
                // add policyXMLSasToken parameter if provided and if the user has provided a policyXMLBaseUrl
                if (exc.policyXMLSasToken != null)
                {
                    TemplateParameterProperties policyTemplateSasTokenProperties = new TemplateParameterProperties()
                    {
                        value = exc.policyXMLSasToken
                    };
                    parameters.Add("PolicyXMLSasToken", policyTemplateSasTokenProperties);
                }
            }
            if (exc.paramServiceUrl)
            {
                Dictionary<string, string> serviceUrls = new Dictionary<string, string>();
                APIExtractor apiExc = new APIExtractor(new FileWriter());
                foreach (string apiName in apisToExtract)
                {
                    string validApiName = ExtractorUtils.GenValidParamName(apiName, "Api");
                    string serviceUrl = GetApiServiceUrlFromParameters(apiName, exc.serviceUrlParameters);
                    if (serviceUrl == null)
                    {
                        serviceUrl = await apiExc.GetAPIServiceUrl(exc.sourceApimName, exc.resourceGroup, apiName);
                    }
                    serviceUrls.Add(validApiName, serviceUrl);
                }
                TemplateServiceUrlProperties serviceUrlProperties = new TemplateServiceUrlProperties()
                {
                    value = serviceUrls
                };
                parameters.Add("serviceUrl", serviceUrlProperties);
            }
            if (exc.paramNamedValue)
            {
                Dictionary<string, string> namedValues = new Dictionary<string, string>();
                PropertyExtractor pExc = new PropertyExtractor();
                string properties = await pExc.GetPropertiesAsync(exc.sourceApimName, exc.resourceGroup);
                JObject oProperties = JObject.Parse(properties);

                foreach (var extractedProperty in oProperties["value"])
                {
                    string propertyName = ((JValue)extractedProperty["name"]).Value.ToString();
                    string fullPropertyResource = await pExc.GetPropertyDetailsAsync(exc.sourceApimName, exc.resourceGroup, propertyName);
                    PropertyTemplateResource propertyTemplateResource = JsonConvert.DeserializeObject<PropertyTemplateResource>(fullPropertyResource);
                    string propertyValue = propertyTemplateResource.properties.value;
                    string validPName = ExtractorUtils.GenValidParamName(propertyName, "Property");
                    namedValues.Add(validPName, propertyValue);
                }
                TemplateServiceUrlProperties namedValueProperties = new TemplateServiceUrlProperties()
                {
                    value = namedValues
                };
                parameters.Add("NamedValues", namedValueProperties);
            }
            if (exc.paramApiLoggerId)
            {
                TemplateServiceUrlProperties loggerIdProperties = new TemplateServiceUrlProperties()
                {
                    value = apiLoggerId
                };
                parameters.Add("ApiLoggerId", loggerIdProperties);
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
            string linkedTemplateUri = linkedTemplatesSasToken != null ? $"parameters('LinkedTemplatesBaseUrl'), '{fileName}', parameters('LinkedTemplatesSasToken')" : $"parameters('LinkedTemplatesBaseUrl'), '{fileName}'";
            return linkedTemplatesUrlQueryString != null ? $"[concat({linkedTemplateUri}, parameters('LinkedTemplatesUrlQueryString'))]" : $"[concat({linkedTemplateUri})]";
        }
    }
}
