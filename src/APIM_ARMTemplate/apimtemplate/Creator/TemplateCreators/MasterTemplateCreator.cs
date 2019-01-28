using System.Collections.Generic;

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
            Template initialAPITemplate,
            Template subsequentAPITemplate,
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
            string initialAPIUri = $"[concat(parameters('LinkedTemplatesBaseUrl'), '{creatorFileNames.initialAPI}')]";
            string[] initialAPIDependsOn = apiVersionSetTemplate != null ? new string[] { "[resourceId('Microsoft.Resources/deployments', 'versionSetTemplate')]" } : new string[] { };
            resources.Add(this.CreateLinkedMasterTemplateResource("initialAPITemplate", initialAPIUri, initialAPIDependsOn));

            string subsequentAPIUri = $"[concat(parameters('LinkedTemplatesBaseUrl'), '{creatorFileNames.subsequentAPI}')]";
            string[] subsequentAPIDependsOn = apiVersionSetTemplate != null ? new string[] { "[resourceId('Microsoft.Resources/deployments', 'initialAPITemplate')]" } : new string[] { };
            resources.Add(this.CreateLinkedMasterTemplateResource("subsequentAPITemplate", subsequentAPIUri, subsequentAPIDependsOn));

            masterTemplate.resources = resources.ToArray();
            return masterTemplate;
        }

        public Template CreateInitialUnlinkedMasterTemplate(Template apiVersionSetTemplate,
            Template initialAPITemplate)
        {
            // create empty template
            Template masterTemplate = this.templateCreator.CreateEmptyTemplate();

            // add parameters
            masterTemplate.parameters = this.CreateMasterTemplateParameters(false);

            // add all resources directly
            List<TemplateResource> resources = new List<TemplateResource>();

            // apiVersionSet
            if (apiVersionSetTemplate != null)
            {
                resources.AddRange(apiVersionSetTemplate.resources);
            }

            //api
            resources.AddRange(initialAPITemplate.resources);

            masterTemplate.resources = resources.ToArray();
            return masterTemplate;
        }

        public Template CreateSubsequentUnlinkedMasterTemplate(Template subequentAPITemplate)
        {
            // create empty template
            Template masterTemplate = this.templateCreator.CreateEmptyTemplate();

            // add parameters
            masterTemplate.parameters = this.CreateMasterTemplateParameters(false);

            // add all resources directly
            List<TemplateResource> resources = new List<TemplateResource>();

            //api
            resources.AddRange(subequentAPITemplate.resources);

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
}
