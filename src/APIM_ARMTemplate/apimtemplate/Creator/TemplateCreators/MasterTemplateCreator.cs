using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class MasterTemplateCreator : TemplateCreator
    {
        public Template CreateLinkedMasterTemplate(CreatorConfig creatorConfig,
            Template globalServicePolicyTemplate,
            Template apiVersionSetTemplate,
            Template productsTemplate,
            Template propertyTemplate,
            Template loggersTemplate,
            Template backendsTemplate,
            Template authorizationServersTemplate,
            Template tagTemplate,
            List<LinkedMasterTemplateAPIInformation> apiInformation,
            FileNames fileNames,
            string apimServiceName,
            FileNameGenerator fileNameGenerator)
        {
            // create empty template
            Template masterTemplate = CreateEmptyTemplate();

            // add parameters
            masterTemplate.parameters = this.CreateMasterTemplateParameters(creatorConfig);

            // add deployment resources that links to all resource files
            List<TemplateResource> resources = new List<TemplateResource>();

            // globalServicePolicy
            if (globalServicePolicyTemplate != null)
            {
                string globalServicePolicyUri = GenerateLinkedTemplateUri(creatorConfig, fileNames.globalServicePolicy);
                resources.Add(this.CreateLinkedMasterTemplateResource("globalServicePolicyTemplate", globalServicePolicyUri, new string[] { }));
            }

            // apiVersionSet
            if (apiVersionSetTemplate != null)
            {
                string apiVersionSetUri = GenerateLinkedTemplateUri(creatorConfig, fileNames.apiVersionSets);
                resources.Add(this.CreateLinkedMasterTemplateResource("versionSetTemplate", apiVersionSetUri, new string[] { }));
            }

            // product
            if (productsTemplate != null)
            {
                string productsUri = GenerateLinkedTemplateUri(creatorConfig, fileNames.products);
                resources.Add(this.CreateLinkedMasterTemplateResource("productsTemplate", productsUri, new string[] { }));
            }

            // property
            if (propertyTemplate != null)
            {
                string propertyUri = GenerateLinkedTemplateUri(creatorConfig, fileNames.namedValues);
                resources.Add(this.CreateLinkedMasterTemplateResource("propertyTemplate", propertyUri, new string[] { }));
            }

            // logger
            if (loggersTemplate != null)
            {
                string loggersUri = GenerateLinkedTemplateUri(creatorConfig, fileNames.loggers);
                resources.Add(this.CreateLinkedMasterTemplateResource("loggersTemplate", loggersUri, new string[] { }));
            }

            // backend
            if (backendsTemplate != null)
            {
                string backendsUri = GenerateLinkedTemplateUri(creatorConfig, fileNames.backends);
                resources.Add(this.CreateLinkedMasterTemplateResource("backendsTemplate", backendsUri, new string[] { }));
            }

            // authorizationServer
            if (authorizationServersTemplate != null)
            {
                string authorizationServersUri = GenerateLinkedTemplateUri(creatorConfig, fileNames.authorizationServers);
                resources.Add(this.CreateLinkedMasterTemplateResource("authorizationServersTemplate", authorizationServersUri, new string[] { }));
            }

            // tag
            if (tagTemplate != null) {
                string tagUri = GenerateLinkedTemplateUri(creatorConfig, fileNames.tags);
                resources.Add(this.CreateLinkedMasterTemplateResource("tagTemplate", tagUri, new string[] { }));
            }

            // each api has an associated api info class that determines whether the api is split and its dependencies on other resources
            foreach (LinkedMasterTemplateAPIInformation apiInfo in apiInformation)
            {
                if (apiInfo.isSplit == true)
                {
                    // add a deployment resource for both api template files
                    string originalAPIName = fileNameGenerator.GenerateOriginalAPIName(apiInfo.name);
                    string initialAPIDeploymentResourceName = $"{originalAPIName}-InitialAPITemplate";
                    string subsequentAPIDeploymentResourceName = $"{originalAPIName}-SubsequentAPITemplate";

                    string initialAPIFileName = fileNameGenerator.GenerateCreatorAPIFileName(apiInfo.name, apiInfo.isSplit, true);
                    string initialAPIUri = GenerateLinkedTemplateUri(creatorConfig, initialAPIFileName);
                    string[] initialAPIDependsOn = CreateAPIResourceDependencies(globalServicePolicyTemplate, apiVersionSetTemplate, productsTemplate, loggersTemplate, backendsTemplate, authorizationServersTemplate, tagTemplate, apiInfo);
                    resources.Add(this.CreateLinkedMasterTemplateResource(initialAPIDeploymentResourceName, initialAPIUri, initialAPIDependsOn));

                    string subsequentAPIFileName = fileNameGenerator.GenerateCreatorAPIFileName(apiInfo.name, apiInfo.isSplit, false);
                    string subsequentAPIUri = GenerateLinkedTemplateUri(creatorConfig, subsequentAPIFileName);
                    string[] subsequentAPIDependsOn = new string[] { $"[resourceId('Microsoft.Resources/deployments', '{initialAPIDeploymentResourceName}')]" };
                    resources.Add(this.CreateLinkedMasterTemplateResource(subsequentAPIDeploymentResourceName, subsequentAPIUri, subsequentAPIDependsOn));
                }
                else
                {
                    // add a deployment resource for the unified api template file
                    string originalAPIName = fileNameGenerator.GenerateOriginalAPIName(apiInfo.name);
                    string unifiedAPIDeploymentResourceName = $"{originalAPIName}-APITemplate";
                    string unifiedAPIFileName = fileNameGenerator.GenerateCreatorAPIFileName(apiInfo.name, apiInfo.isSplit, true);
                    string unifiedAPIUri = GenerateLinkedTemplateUri(creatorConfig, unifiedAPIFileName);
                    string[] unifiedAPIDependsOn = CreateAPIResourceDependencies(globalServicePolicyTemplate, apiVersionSetTemplate, productsTemplate, loggersTemplate, backendsTemplate, authorizationServersTemplate, tagTemplate, apiInfo);
                    resources.Add(this.CreateLinkedMasterTemplateResource(unifiedAPIDeploymentResourceName, unifiedAPIUri, unifiedAPIDependsOn));
                }
            }

            masterTemplate.resources = resources.ToArray();
            return masterTemplate;
        }

        public string[] CreateAPIResourceDependencies(Template globalServicePolicyTemplate,
            Template apiVersionSetTemplate,
            Template productsTemplate,
            Template loggersTemplate,
            Template backendsTemplate,
            Template authorizationServersTemplate,
            Template tagTemplate,
            LinkedMasterTemplateAPIInformation apiInfo)
        {
            List<string> apiDependsOn = new List<string>();
            if (globalServicePolicyTemplate != null && apiInfo.dependsOnGlobalServicePolicies == true)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'globalServicePolicyTemplate')]");
            }
            if (apiVersionSetTemplate != null && apiInfo.dependsOnVersionSets == true)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'versionSetTemplate')]");
            }
            if (productsTemplate != null && apiInfo.dependsOnProducts == true)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'productsTemplate')]");
            }
            if (loggersTemplate != null && apiInfo.dependsOnLoggers == true)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'loggersTemplate')]");
            }
            if (backendsTemplate != null && apiInfo.dependsOnBackends == true)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'backendsTemplate')]");
            }
            if (authorizationServersTemplate != null && apiInfo.dependsOnAuthorizationServers == true)
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'authorizationServersTemplate')]");
            }
            if (tagTemplate != null && apiInfo.dependsOnTags == true) 
            {
                apiDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'tagTemplate')]");
            }
            return apiDependsOn.ToArray();
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

        public Dictionary<string, TemplateParameterProperties> CreateMasterTemplateParameters(CreatorConfig creatorConfig)
        {
            // used to create the parameter metatadata, etc (not value) for use in file with resources
            // add parameters with metadata properties
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
            if (creatorConfig.linked == true)
            {
                TemplateParameterProperties linkedTemplatesBaseUrlProperties = new TemplateParameterProperties()
                {
                    metadata = new TemplateParameterMetadata()
                    {
                        description = "Base URL of the repository"
                    },
                    type = "string"
                };
                parameters.Add(ParameterNames.LinkedTemplatesBaseUrl, linkedTemplatesBaseUrlProperties);
                if (creatorConfig.linkedTemplatesUrlQueryString != null)
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
            return parameters;
        }

        public Template CreateMasterTemplateParameterValues(CreatorConfig creatorConfig)
        {
            // used to create the parameter values for use in parameters file
            // create empty template
            Template masterTemplate = CreateEmptyParameters();

            // add parameters with value property
            Dictionary<string, TemplateParameterProperties> parameters = new Dictionary<string, TemplateParameterProperties>();
            TemplateParameterProperties apimServiceNameProperties = new TemplateParameterProperties()
            {
                value = creatorConfig.apimServiceName
            };
            parameters.Add(ParameterNames.ApimServiceName, apimServiceNameProperties);
            if (creatorConfig.linked == true)
            {
                TemplateParameterProperties linkedTemplatesBaseUrlProperties = new TemplateParameterProperties()
                {
                    value = creatorConfig.linkedTemplatesBaseUrl
                };
                parameters.Add(ParameterNames.LinkedTemplatesBaseUrl, linkedTemplatesBaseUrlProperties);
                if (creatorConfig.linkedTemplatesUrlQueryString != null)
                {
                    TemplateParameterProperties linkedTemplatesUrlQueryStringProperties = new TemplateParameterProperties()
                    {
                        value = creatorConfig.linkedTemplatesUrlQueryString
                    };
                    parameters.Add(ParameterNames.LinkedTemplatesUrlQueryString, linkedTemplatesUrlQueryStringProperties);
                }
            }
            masterTemplate.parameters = parameters;
            return masterTemplate;
        }

        public async Task<bool> DetermineIfAPIDependsOnLoggerAsync(APIConfig api, FileReader fileReader)
        {
            if (api.diagnostic != null && api.diagnostic.loggerId != null)
            {
                // capture api diagnostic dependent on logger
                return true;
            }
            string apiPolicy = api.policy != null ? await fileReader.RetrieveFileContentsAsync(api.policy) : "";
            if (apiPolicy.Contains("logger"))
            {
                // capture api policy dependent on logger
                return true;
            }
            if (api.operations != null)
            {
                foreach (KeyValuePair<string, OperationsConfig> operation in api.operations)
                {
                    string operationPolicy = operation.Value.policy != null ? await fileReader.RetrieveFileContentsAsync(operation.Value.policy) : "";
                    if (operationPolicy.Contains("logger"))
                    {
                        // capture operation policy dependent on logger
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<bool> DetermineIfAPIDependsOnBackendAsync(APIConfig api, FileReader fileReader)
        {
            string apiPolicy = api.policy != null ? await fileReader.RetrieveFileContentsAsync(api.policy) : "";
            if (apiPolicy.Contains("set-backend-service"))
            {
                // capture api policy dependent on backend
                return true;
            }
            if (api.operations != null)
            {
                foreach (KeyValuePair<string, OperationsConfig> operation in api.operations)
                {
                    string operationPolicy = operation.Value.policy != null ? await fileReader.RetrieveFileContentsAsync(operation.Value.policy) : "";
                    if (operationPolicy.Contains("set-backend-service"))
                    {
                        // capture operation policy dependent on backend
                        return true;
                    }
                }
            }
            return false;
        }

        public string GenerateLinkedTemplateUri(CreatorConfig creatorConfig, string fileName)
        {
            return creatorConfig.linkedTemplatesUrlQueryString != null ?
             $"[concat(parameters('{ParameterNames.LinkedTemplatesBaseUrl}'), '{fileName}', parameters('{ParameterNames.LinkedTemplatesUrlQueryString}'))]" 
             : $"[concat(parameters('{ParameterNames.LinkedTemplatesBaseUrl}'), '{fileName}')]";
        }
    }

    public class LinkedMasterTemplateAPIInformation
    {
        public string name { get; set; }
        public bool isSplit { get; set; }
        public bool dependsOnGlobalServicePolicies { get; set; }
        public bool dependsOnVersionSets { get; set; }
        public bool dependsOnProducts { get; set; }
        public bool dependsOnLoggers { get; set; }
        public bool dependsOnBackends { get; set; }
        public bool dependsOnAuthorizationServers { get; set; }
        public bool dependsOnTags { get; set; }
    }

}
