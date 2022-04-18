// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Master;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class MasterTemplateCreator
    {
        readonly ITemplateBuilder templateBuilder;

        public MasterTemplateCreator(ITemplateBuilder templateBuilder)
        {
            this.templateBuilder = templateBuilder;
        }

        public Template CreateLinkedMasterTemplate(CreatorConfig creatorConfig,
            Template globalServicePolicyTemplate,
            Template apiVersionSetTemplate,
            Template productsTemplate,
            Template productAPIsTemplate,
            Template propertyTemplate,
            Template loggersTemplate,
            Template backendsTemplate,
            Template authorizationServersTemplate,
            Template tagTemplate,
            List<LinkedMasterTemplateAPIInformation> apiInformation,
            FileNames fileNames,
            string apimServiceName)
        {
            // create empty template
            Template masterTemplate = this.templateBuilder.GenerateEmptyTemplate().Build();

            // add parameters
            masterTemplate.Parameters = this.CreateMasterTemplateParameters(creatorConfig);

            // add deployment resources that links to all resource files
            List<TemplateResource> resources = new List<TemplateResource>();

            // globalServicePolicy
            if (globalServicePolicyTemplate != null)
            {
                string globalServicePolicyUri = this.GenerateLinkedTemplateUri(creatorConfig, fileNames.GlobalServicePolicy);
                resources.Add(this.CreateLinkedMasterTemplateResource("globalServicePolicyTemplate", globalServicePolicyUri, new string[] { }, null, false));
            }

            // apiVersionSet
            if (apiVersionSetTemplate != null)
            {
                string apiVersionSetUri = this.GenerateLinkedTemplateUri(creatorConfig, fileNames.ApiVersionSets);
                resources.Add(this.CreateLinkedMasterTemplateResource("versionSetTemplate", apiVersionSetUri, new string[] { }, null, false));
            }

            // product
            if (productsTemplate != null)
            {
                string productsUri = this.GenerateLinkedTemplateUri(creatorConfig, fileNames.Products);
                resources.Add(this.CreateLinkedMasterTemplateResource("productsTemplate", productsUri, new string[] { }, null, false));
            }

            // productApi
            if (productAPIsTemplate != null)
            {
                // depends on all products and APIs
                string[] dependsOn = this.CreateProductAPIResourceDependencies(productsTemplate, apiInformation);
                string productAPIsUri = this.GenerateLinkedTemplateUri(creatorConfig, fileNames.ProductAPIs);
                resources.Add(this.CreateLinkedMasterTemplateResource("productAPIsTemplate", productAPIsUri, dependsOn, null, false));
            }

            // property
            if (propertyTemplate != null)
            {
                string propertyUri = this.GenerateLinkedTemplateUri(creatorConfig, fileNames.NamedValues);
                resources.Add(this.CreateLinkedMasterTemplateResource("propertyTemplate", propertyUri, new string[] { }, null, false));
            }

            // logger
            if (loggersTemplate != null)
            {
                string loggersUri = this.GenerateLinkedTemplateUri(creatorConfig, fileNames.Loggers);
                resources.Add(this.CreateLinkedMasterTemplateResource("loggersTemplate", loggersUri, new string[] { }, null, false));
            }

            // backend
            if (backendsTemplate != null)
            {
                string backendsUri = this.GenerateLinkedTemplateUri(creatorConfig, fileNames.Backends);
                resources.Add(this.CreateLinkedMasterTemplateResource("backendsTemplate", backendsUri, new string[] { }, null, false));
            }

            // authorizationServer
            if (authorizationServersTemplate != null)
            {
                string authorizationServersUri = this.GenerateLinkedTemplateUri(creatorConfig, fileNames.AuthorizationServers);
                resources.Add(this.CreateLinkedMasterTemplateResource("authorizationServersTemplate", authorizationServersUri, new string[] { }, null, false));
            }

            // tag
            if (tagTemplate != null)
            {
                string tagUri = this.GenerateLinkedTemplateUri(creatorConfig, fileNames.Tags);
                resources.Add(this.CreateLinkedMasterTemplateResource("tagTemplate", tagUri, new string[] { }, null, false));
            }

            string previousAPIName = null;
            // each api has an associated api info class that determines whether the api is split and its dependencies on other resources
            foreach (LinkedMasterTemplateAPIInformation apiInfo in apiInformation)
            {
                if (apiInfo.isSplit == true)
                {
                    // add a deployment resource for both api template files
                    string originalAPIName = FileNameGenerator.GenerateOriginalAPIName(apiInfo.name);
                    string subsequentAPIDeploymentResourceName = $"{originalAPIName}-SubsequentAPITemplate";
                    string initialAPIDeploymentResourceName = $"{originalAPIName}-InitialAPITemplate";

                    string initialAPIFileName = FileNameGenerator.GenerateCreatorAPIFileName(apiInfo.name, apiInfo.isSplit, true);
                    string initialAPIUri = this.GenerateLinkedTemplateUri(creatorConfig, initialAPIFileName);
                    string[] initialAPIDependsOn = this.CreateAPIResourceDependencies(creatorConfig, globalServicePolicyTemplate, apiVersionSetTemplate, productsTemplate, loggersTemplate, backendsTemplate, authorizationServersTemplate, tagTemplate, apiInfo, previousAPIName);
                    resources.Add(this.CreateLinkedMasterTemplateResource(initialAPIDeploymentResourceName, initialAPIUri, initialAPIDependsOn, originalAPIName, apiInfo.isServiceUrlParameterize));

                    string subsequentAPIFileName = FileNameGenerator.GenerateCreatorAPIFileName(apiInfo.name, apiInfo.isSplit, false);
                    string subsequentAPIUri = this.GenerateLinkedTemplateUri(creatorConfig, subsequentAPIFileName);
                    string[] subsequentAPIDependsOn = new string[] { $"[resourceId('Microsoft.Resources/deployments', '{initialAPIDeploymentResourceName}')]" };
                    resources.Add(this.CreateLinkedMasterTemplateResource(subsequentAPIDeploymentResourceName, subsequentAPIUri, subsequentAPIDependsOn, originalAPIName, apiInfo.isServiceUrlParameterize));

                    // Set previous API name for dependency chain
                    previousAPIName = subsequentAPIDeploymentResourceName;
                }
                else
                {
                    // add a deployment resource for the unified api template file
                    string originalAPIName = FileNameGenerator.GenerateOriginalAPIName(apiInfo.name);
                    string subsequentAPIDeploymentResourceName = $"{originalAPIName}-SubsequentAPITemplate";
                    string unifiedAPIDeploymentResourceName = $"{originalAPIName}-APITemplate";
                    string unifiedAPIFileName = FileNameGenerator.GenerateCreatorAPIFileName(apiInfo.name, apiInfo.isSplit, true);
                    string unifiedAPIUri = this.GenerateLinkedTemplateUri(creatorConfig, unifiedAPIFileName);
                    string[] unifiedAPIDependsOn = this.CreateAPIResourceDependencies(creatorConfig, globalServicePolicyTemplate, apiVersionSetTemplate, productsTemplate, loggersTemplate, backendsTemplate, authorizationServersTemplate, tagTemplate, apiInfo, previousAPIName);
                    resources.Add(this.CreateLinkedMasterTemplateResource(unifiedAPIDeploymentResourceName, unifiedAPIUri, unifiedAPIDependsOn, originalAPIName, apiInfo.isServiceUrlParameterize));

                    // Set previous API name for dependency chain
                    previousAPIName = subsequentAPIDeploymentResourceName;
                }
            }

            masterTemplate.Resources = resources.ToArray();
            return masterTemplate;
        }

        public string[] CreateAPIResourceDependencies(
            CreatorConfig creatorConfig,
            Template globalServicePolicyTemplate,
            Template apiVersionSetTemplate,
            Template productsTemplate,
            Template loggersTemplate,
            Template backendsTemplate,
            Template authorizationServersTemplate,
            Template tagTemplate,
            LinkedMasterTemplateAPIInformation apiInfo,
            string previousAPI)
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
            if (apiInfo.dependsOnVersion != null)
            {
                apiDependsOn.Add($"[resourceId('Microsoft.Resources/deployments', '{apiInfo.dependsOnVersion}-SubsequentAPITemplate')]");
            }
            if (previousAPI != null && apiInfo.dependsOnTags == true)
            {
                apiDependsOn.Add($"[resourceId('Microsoft.Resources/deployments', '{previousAPI}')]");
            }
            return apiDependsOn.ToArray();
        }

        public string[] CreateProductAPIResourceDependencies(Template productsTemplate,
            List<LinkedMasterTemplateAPIInformation> apiInformation)
        {
            List<string> apiProductDependsOn = new List<string>();
            if (productsTemplate != null)
            {
                apiProductDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'productsTemplate')]");
            }
            foreach (LinkedMasterTemplateAPIInformation apiInfo in apiInformation)
            {
                string originalAPIName = FileNameGenerator.GenerateOriginalAPIName(apiInfo.name);
                string apiDeploymentResourceName = apiInfo.isSplit ? $"{originalAPIName}-SubsequentAPITemplate" : $"{originalAPIName}-APITemplate";
                apiProductDependsOn.Add($"[resourceId('Microsoft.Resources/deployments', '{apiDeploymentResourceName}')]");
            }
            return apiProductDependsOn.ToArray();
        }

        public MasterTemplateResource CreateLinkedMasterTemplateResource(string name, string uriLink, string[] dependsOn, string apiName, bool isServiceUrlParameterizeInApi)
        {
            // create deployment resource with provided arguments
            MasterTemplateResource masterTemplateResource = new MasterTemplateResource()
            {
                Name = name,
                Type = "Microsoft.Resources/deployments",
                ApiVersion = GlobalConstants.ArmApiVersion,
                Properties = new MasterTemplateProperties()
                {
                    Mode = "Incremental",
                    TemplateLink = new MasterTemplateLink()
                    {
                        Uri = uriLink,
                        ContentVersion = "1.0.0.0"
                    },
                    Parameters = new Dictionary<string, TemplateParameterProperties>
                    {
                        { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ Value = $"[parameters('{ParameterNames.ApimServiceName}')]" } }
                    }
                },
                DependsOn = dependsOn
            };

            if (name.IndexOf("APITemplate") > 0 && isServiceUrlParameterizeInApi)
            {
                TemplateParameterProperties serviceUrlParamProperty = new TemplateParameterProperties()
                {
                    Value = $"[parameters('{apiName}-ServiceUrl')]"
                };
                masterTemplateResource.Properties.Parameters.Add(apiName + "-ServiceUrl", serviceUrlParamProperty);
            }

            return masterTemplateResource;
        }

        public string GetDependsOnPreviousApiVersion(APIConfig api, IDictionary<string, string[]> apiVersions)
        {
            if (api?.apiVersionSetId == null)
                return null;

            // get all apis associated with the same versionSet
            // versions must be deployed in sequence and thus
            // each api must depend on the previous version.

            var versions = apiVersions.ContainsKey(api.apiVersionSetId)
                ? apiVersions[api.apiVersionSetId]
                : null
                ;

            var index = Array.IndexOf(versions, api.name);
            var previous = index > 0
                ? (int?)index - 1
                : null
                ;

            return previous.HasValue
                ? versions[previous.Value]
                : null
                ;
        }

        public Dictionary<string, TemplateParameterProperties> CreateMasterTemplateParameters(CreatorConfig creatorConfig)
        {
            // used to create the parameter metatadata, etc (not value) for use in file with resources
            // add parameters with metadata properties
            Dictionary<string, TemplateParameterProperties> parameters = new Dictionary<string, TemplateParameterProperties>();
            TemplateParameterProperties apimServiceNameProperties = new TemplateParameterProperties()
            {
                Metadata = new TemplateParameterMetadata()
                {
                    Description = "Name of the API Management"
                },
                Type = "string"
            };
            parameters.Add(ParameterNames.ApimServiceName, apimServiceNameProperties);
            // add remote location of template files for linked option
            if (creatorConfig.linked == true)
            {
                TemplateParameterProperties linkedTemplatesBaseUrlProperties = new TemplateParameterProperties()
                {
                    Metadata = new TemplateParameterMetadata()
                    {
                        Description = "Base URL of the repository"
                    },
                    Type = "string"
                };
                parameters.Add(ParameterNames.LinkedTemplatesBaseUrl, linkedTemplatesBaseUrlProperties);
                if (creatorConfig.linkedTemplatesUrlQueryString != null)
                {
                    TemplateParameterProperties linkedTemplatesUrlQueryStringProperties = new TemplateParameterProperties()
                    {
                        Metadata = new TemplateParameterMetadata()
                        {
                            Description = "Query string for the URL of the repository"
                        },
                        Type = "string"
                    };
                    parameters.Add(ParameterNames.LinkedTemplatesUrlQueryString, linkedTemplatesUrlQueryStringProperties);
                }
            }

            // add serviceUrl parameter for linked option
            if (creatorConfig.serviceUrlParameters != null && creatorConfig.serviceUrlParameters.Count > 0)
            {
                foreach (var serviceUrlProperty in creatorConfig.serviceUrlParameters)
                {
                    TemplateParameterProperties serviceUrlParamProperty = new TemplateParameterProperties()
                    {
                        Metadata = new TemplateParameterMetadata()
                        {
                            Description = "ServiceUrl parameter for API: " + serviceUrlProperty.ApiName
                        },
                        Type = "string"
                    };
                    parameters.Add(serviceUrlProperty.ApiName + "-ServiceUrl", serviceUrlParamProperty);
                }
            }

            return parameters;
        }

        public Template CreateMasterTemplateParameterValues(CreatorConfig creatorConfig)
        {
            // used to create the parameter values for use in parameters file
            // create empty template
            Template masterTemplate = this.templateBuilder.GenerateEmptyTemplate().Build();

            // add parameters with value property
            Dictionary<string, TemplateParameterProperties> parameters = new Dictionary<string, TemplateParameterProperties>();
            TemplateParameterProperties apimServiceNameProperties = new TemplateParameterProperties()
            {
                Value = creatorConfig.apimServiceName
            };
            parameters.Add(ParameterNames.ApimServiceName, apimServiceNameProperties);
            if (creatorConfig.linked == true)
            {
                TemplateParameterProperties linkedTemplatesBaseUrlProperties = new TemplateParameterProperties()
                {
                    Value = creatorConfig.linkedTemplatesBaseUrl
                };
                parameters.Add(ParameterNames.LinkedTemplatesBaseUrl, linkedTemplatesBaseUrlProperties);
                if (creatorConfig.linkedTemplatesUrlQueryString != null)
                {
                    TemplateParameterProperties linkedTemplatesUrlQueryStringProperties = new TemplateParameterProperties()
                    {
                        Value = creatorConfig.linkedTemplatesUrlQueryString
                    };
                    parameters.Add(ParameterNames.LinkedTemplatesUrlQueryString, linkedTemplatesUrlQueryStringProperties);
                }
            }

            if (creatorConfig.serviceUrlParameters != null && creatorConfig.serviceUrlParameters.Count > 0)
            {
                foreach (var serviceUrlProperty in creatorConfig.serviceUrlParameters)
                {
                    TemplateParameterProperties serviceUrlParamProperty = new TemplateParameterProperties()
                    {
                        Value = serviceUrlProperty.ServiceUrl
                    };
                    parameters.Add(serviceUrlProperty.ApiName + "-ServiceUrl", serviceUrlParamProperty);
                }
            }

            masterTemplate.Parameters = parameters;
            return masterTemplate;
        }

        public async Task<bool> DetermineIfAPIDependsOnLoggerAsync(APIConfig api, FileReader fileReader)
        {
            if (api.diagnostic != null && api.diagnostic.LoggerId != null)
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
        public bool isServiceUrlParameterize { get; set; }
        public string dependsOnVersion { get; set; }
    }

}
