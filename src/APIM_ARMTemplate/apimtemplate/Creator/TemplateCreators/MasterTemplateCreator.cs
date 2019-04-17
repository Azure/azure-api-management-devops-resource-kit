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
            List<LinkedMasterTemplateAPIInformation> apiInformation,
            CreatorFileNames creatorFileNames)
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
                string apiVersionSetUri = $"[concat(parameters('LinkedTemplatesBaseUrl'), '{creatorFileNames.apiVersionSet}')]";
                resources.Add(this.CreateLinkedMasterTemplateResource("versionSetTemplate", apiVersionSetUri, new string[] { }));
            }

            //api
            foreach(LinkedMasterTemplateAPIInformation apiInfo in apiInformation)
            {
                string initialAPIFileName = $@"/{apiInfo.name}-initial.api.template.json";
                string initialAPIUri = $"[concat(parameters('LinkedTemplatesBaseUrl'), '{initialAPIFileName}')]";
                string[] initialAPIDependsOn = apiVersionSetTemplate != null && apiInfo.hasAPIVersionSetId == true ? new string[] { "[resourceId('Microsoft.Resources/deployments', 'versionSetTemplate')]" } : new string[] { };
                resources.Add(this.CreateLinkedMasterTemplateResource("initialAPITemplate", initialAPIUri, initialAPIDependsOn));

                string subsequentAPIFileName = $@"/{apiInfo.name}-subsequent.api.template.json";
                string subsequentAPIUri = $"[concat(parameters('LinkedTemplatesBaseUrl'), '{subsequentAPIFileName}')]";
                string[] subsequentAPIDependsOn = new string[] { "[resourceId('Microsoft.Resources/deployments', 'initialAPITemplate')]" };
                resources.Add(this.CreateLinkedMasterTemplateResource("subsequentAPITemplate", subsequentAPIUri, subsequentAPIDependsOn));
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
            if(linked == true)
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
    }

    public class LinkedMasterTemplateAPIInformation
    {

        public string name { get; set; }
        public bool hasAPIVersionSetId { get; set; }
    }
}
