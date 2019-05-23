using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class MasterTemplateCreator : TemplateCreator
    {
        public Template CreateLinkedMasterTemplate(Template apiVersionSetTemplate,
            Template productsTemplate,
            Template loggersTemplate,
            Template backendsTemplate,
            Template authorizationServersTemplate,
            List<LinkedMasterTemplateAPIInformation> apiInformation,
            FileNames fileNames,
            string apimServiceName,
            FileNameGenerator fileNameGenerator)
        {
            // create empty template
            Template masterTemplate = CreateEmptyTemplate();

            // add parameters
            masterTemplate.parameters = this.CreateMasterTemplateParameters(true);

            // add deployment resources that links to all resource files
            List<TemplateResource> resources = new List<TemplateResource>();

            // apiVersionSet
            if (apiVersionSetTemplate != null)
            {
                string apiVersionSetUri = $"[concat(parameters('LinkedTemplatesBaseUrl'), '{fileNames.apiVersionSets}')]";
                resources.Add(this.CreateLinkedMasterTemplateResource("versionSetTemplate", apiVersionSetUri, new string[] { }));
            }

            // product
            if (productsTemplate != null)
            {
                string productsUri = $"[concat(parameters('LinkedTemplatesBaseUrl'), '{fileNames.products}')]";
                resources.Add(this.CreateLinkedMasterTemplateResource("productsTemplate", productsUri, new string[] { }));
            }

            // logger
            if (loggersTemplate != null)
            {
                string loggersUri = $"[concat(parameters('LinkedTemplatesBaseUrl'), '{fileNames.loggers}')]";
                resources.Add(this.CreateLinkedMasterTemplateResource("loggersTemplate", loggersUri, new string[] { }));
            }

            // backend
            if (backendsTemplate != null)
            {
                string backendsUri = $"[concat(parameters('LinkedTemplatesBaseUrl'), '{fileNames.backends}')]";
                resources.Add(this.CreateLinkedMasterTemplateResource("backendsTemplate", backendsUri, new string[] { }));
            }

            // authorizationServer
            if (authorizationServersTemplate != null)
            {
                string authorizationServersUri = $"[concat(parameters('LinkedTemplatesBaseUrl'), '{fileNames.authorizationServers}')]";
                resources.Add(this.CreateLinkedMasterTemplateResource("authorizationServersTemplate", authorizationServersUri, new string[] { }));
            }

            // each api has an associated api info class that determines whether the api is split and its dependencies on other resources
            foreach (LinkedMasterTemplateAPIInformation apiInfo in apiInformation)
            {
                if(apiInfo.isSplit == true)
                {
                    // add a deployment resource for both api template files
                    string originalAPIName = fileNameGenerator.GenerateOriginalAPIName(apiInfo.name);
                    string initialAPIDeploymentResourceName = $"{originalAPIName}-InitialAPITemplate";
                    string subsequentAPIDeploymentResourceName = $"{originalAPIName}-SubsequentAPITemplate";

                    string initialAPIFileName = fileNameGenerator.GenerateCreatorAPIFileName(apiInfo.name, apiInfo.isSplit, true, apimServiceName);
                    string initialAPIUri = $"[concat(parameters('LinkedTemplatesBaseUrl'), '{initialAPIFileName}')]";
                    string[] initialAPIDependsOn = CreateAPIResourceDependencies(apiVersionSetTemplate, productsTemplate, loggersTemplate, backendsTemplate, authorizationServersTemplate, apiInfo);
                    resources.Add(this.CreateLinkedMasterTemplateResource(initialAPIDeploymentResourceName, initialAPIUri, initialAPIDependsOn));

                    string subsequentAPIFileName = fileNameGenerator.GenerateCreatorAPIFileName(apiInfo.name, apiInfo.isSplit, false, apimServiceName);
                    string subsequentAPIUri = $"[concat(parameters('LinkedTemplatesBaseUrl'), '{subsequentAPIFileName}')]";
                    string[] subsequentAPIDependsOn = new string[] { $"[resourceId('Microsoft.Resources/deployments', '{initialAPIDeploymentResourceName}')]" };
                    resources.Add(this.CreateLinkedMasterTemplateResource(subsequentAPIDeploymentResourceName, subsequentAPIUri, subsequentAPIDependsOn));
                } else
                {
                    // add a deployment resource for the unified api template file
                    string originalAPIName = fileNameGenerator.GenerateOriginalAPIName(apiInfo.name);
                    string unifiedAPIDeploymentResourceName = $"{originalAPIName}-APITemplate";
                    string unifiedAPIFileName = fileNameGenerator.GenerateCreatorAPIFileName(apiInfo.name, apiInfo.isSplit, true, apimServiceName);
                    string unifiedAPIUri = $"[concat(parameters('LinkedTemplatesBaseUrl'), '{unifiedAPIFileName}')]";
                    string[] unifiedAPIDependsOn = CreateAPIResourceDependencies(apiVersionSetTemplate, productsTemplate, loggersTemplate, backendsTemplate, authorizationServersTemplate, apiInfo);
                    resources.Add(this.CreateLinkedMasterTemplateResource(unifiedAPIDeploymentResourceName, unifiedAPIUri, unifiedAPIDependsOn));
                }
            }

            masterTemplate.resources = resources.ToArray();
            return masterTemplate;
        }

        public string[] CreateAPIResourceDependencies(Template apiVersionSetTemplate,
            Template productsTemplate,
            Template loggersTemplate,
            Template backendsTemplate,
            Template authorizationServersTemplate,
            LinkedMasterTemplateAPIInformation apiInfo)
        {
            List<string> apiDependsOn = new List<string>();
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
                        { "ApimServiceName", new TemplateParameterProperties(){ value = "[parameters('ApimServiceName')]" } }
                    }
                },
                dependsOn = dependsOn
            };
            return masterTemplateResource;
        }

        public Dictionary<string, TemplateParameterProperties> CreateMasterTemplateParameters(bool linked)
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
                        description = "Base URL of the repository"
                    },
                    type = "string"
                };
                parameters.Add("LinkedTemplatesBaseUrl", linkedTemplatesBaseUrlProperties);
            }
            return parameters;
        }

        public Template CreateMasterTemplateParameterValues(CreatorConfig creatorConfig)
        {
            // used to create the parameter values for use in parameters file
            // create empty template
            Template masterTemplate = CreateEmptyTemplate();

            // add parameters with value property
            Dictionary<string, TemplateParameterProperties> parameters = new Dictionary<string, TemplateParameterProperties>();
            TemplateParameterProperties apimServiceNameProperties = new TemplateParameterProperties()
            {
                value = creatorConfig.apimServiceName
            };
            parameters.Add("ApimServiceName", apimServiceNameProperties);
            if (creatorConfig.linked == true)
            {
                TemplateParameterProperties linkedTemplatesBaseUrlProperties = new TemplateParameterProperties()
                {
                    value = creatorConfig.linkedTemplatesBaseUrl
                };
                parameters.Add("LinkedTemplatesBaseUrl", linkedTemplatesBaseUrlProperties);
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
                    string operationPolicy = operation.Value.policy != null ?  await fileReader.RetrieveFileContentsAsync(operation.Value.policy) : "";
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
    }

    public class LinkedMasterTemplateAPIInformation
    {
        public string name { get; set; }
        public bool isSplit { get; set; }
        public bool dependsOnVersionSets { get; set; }
        public bool dependsOnProducts { get; set; }
        public bool dependsOnLoggers { get; set; }
        public bool dependsOnBackends { get; set; }
        public bool dependsOnAuthorizationServers { get; set; }
    }

}
