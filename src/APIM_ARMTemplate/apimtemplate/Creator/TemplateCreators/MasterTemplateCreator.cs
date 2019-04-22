using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class MasterTemplateCreator
    {
        private TemplateCreator templateCreator;

        public MasterTemplateCreator(TemplateCreator templateCreator)
        {
            this.templateCreator = templateCreator;
        }

        public Template CreateLinkedMasterTemplate(Template apiVersionSetTemplate,
            Template productsTemplate,
            Template loggersTemplate,
            List<LinkedMasterTemplateAPIInformation> apiInformation,
            CreatorFileNames creatorFileNames,
            FileNameGenerator fileNameGenerator)
        {
            // create empty template
            Template masterTemplate = this.templateCreator.CreateEmptyTemplate();

            // add parameters
            masterTemplate.parameters = this.CreateMasterTemplateParameters(true);

            // add links to all resources
            List<TemplateResource> resources = new List<TemplateResource>();

            // apiVersionSet
            if (apiVersionSetTemplate != null)
            {
                string apiVersionSetUri = $"[concat(parameters('LinkedTemplatesBaseUrl'), '{creatorFileNames.apiVersionSets}')]";
                resources.Add(this.CreateLinkedMasterTemplateResource("versionSetTemplate", apiVersionSetUri, new string[] { }));
            }

            // product
            if (productsTemplate != null)
            {
                string productsUri = $"[concat(parameters('LinkedTemplatesBaseUrl'), '{creatorFileNames.products}')]";
                resources.Add(this.CreateLinkedMasterTemplateResource("productsTemplate", productsUri, new string[] { }));
            }

            // logger
            if (loggersTemplate != null)
            {
                string loggersUri = $"[concat(parameters('LinkedTemplatesBaseUrl'), '{creatorFileNames.loggers}')]";
                resources.Add(this.CreateLinkedMasterTemplateResource("loggersTemplate", loggersUri, new string[] { }));
            }

            // api info stores api name as the key and whether the api depends on the version set template as the value
            foreach (LinkedMasterTemplateAPIInformation apiInfo in apiInformation)
            {
                string initialAPIDeploymentResourceName = $"{apiInfo.name}-InitialAPITemplate";
                string subsequentAPIDeploymentResourceName = $"{apiInfo.name}-SubsequentAPITemplate";

                string initialAPIFileName = fileNameGenerator.GenerateAPIFileName(apiInfo.name, true);
                string initialAPIUri = $"[concat(parameters('LinkedTemplatesBaseUrl'), '{initialAPIFileName}')]";
                List<string> initialAPIDependsOn = new List<string>();
                if (apiVersionSetTemplate != null && apiInfo.dependsOnVersionSets == true)
                {
                    initialAPIDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'versionSetTemplate')]");
                }
                if (productsTemplate != null && apiInfo.dependsOnProducts == true)
                {
                    initialAPIDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'productsTemplate')]");
                }
                if (loggersTemplate != null && apiInfo.dependsOnLoggers == true)
                {
                    initialAPIDependsOn.Add("[resourceId('Microsoft.Resources/deployments', 'loggersTemplate')]");
                }
                resources.Add(this.CreateLinkedMasterTemplateResource(initialAPIDeploymentResourceName, initialAPIUri, initialAPIDependsOn.ToArray()));

                string subsequentAPIFileName = fileNameGenerator.GenerateAPIFileName(apiInfo.name, false);
                string subsequentAPIUri = $"[concat(parameters('LinkedTemplatesBaseUrl'), '{subsequentAPIFileName}')]";
                string[] subsequentAPIDependsOn = new string[] { $"[resourceId('Microsoft.Resources/deployments', '{initialAPIDeploymentResourceName}')]" };
                resources.Add(this.CreateLinkedMasterTemplateResource(subsequentAPIDeploymentResourceName, subsequentAPIUri, subsequentAPIDependsOn));
            }

            masterTemplate.resources = resources.ToArray();
            return masterTemplate;
        }

        public MasterTemplateResource CreateLinkedMasterTemplateResource(string name, string uriLink, string[] dependsOn)
        {
            MasterTemplateResource masterTemplateResource = new MasterTemplateResource()
            {
                name = name,
                type = "Microsoft.Resources/deployments",
                apiVersion = "2018-01-01",
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
            Template masterTemplate = this.templateCreator.CreateEmptyTemplate();

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

        public bool DetermineIfAPIDependsOnLogger(APIConfig api, FileReader fileReader)
        {
            if (api.diagnostic != null && api.diagnostic.loggerId != null)
            {
                // capture api diagnostic dependent on logger
                return true;
            }
            string apiPolicy = api.policy != null ? fileReader.RetrieveLocalFileContents(api.policy) : "";
            if (apiPolicy.Contains("logger"))
            {
                // capture api policy dependent on logger
                return true;
            }
            if (api.operations != null)
            {
                foreach (KeyValuePair<string, OperationsConfig> operation in api.operations)
                {
                    string operationPolicy = operation.Value.policy != null ? fileReader.RetrieveLocalFileContents(operation.Value.policy) : "";
                    if (operation.Value.policy != null && operation.Value.policy.Contains("logger"))
                    {
                        // capture operation policy dependent on logger
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
        public bool dependsOnVersionSets { get; set; }
        public bool dependsOnProducts { get; set; }
        public bool dependsOnLoggers { get; set; }
    }

}
