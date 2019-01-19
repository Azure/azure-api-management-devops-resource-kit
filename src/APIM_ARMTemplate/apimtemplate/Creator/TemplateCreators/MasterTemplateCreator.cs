using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class MasterTemplateCreator
    {
        private TemplateCreator templateCreator;

        public MasterTemplateCreator(TemplateCreator templateCreator)
        {
            this.templateCreator = templateCreator;
        }

        public Template CreateLinkedMasterTemplate(Template apiVersionSetTemplate,
            Template apiTemplate,
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
                resources.Add(this.CreateMasterTemplateResource("versionSetTemplate", apiVersionSetUri, new string[] { }));
            }

            //api
            string initialAPIUri = $"[concat(parameters('LinkedTemplatesBaseUrl'), '{creatorFileNames.api}')]";
            string[] initialAPIDependsOn = apiVersionSetTemplate != null ? new string[] { "[resourceId('Microsoft.Resources/deployments', 'versionSetTemplate')]" } : new string[] { };
            resources.Add(this.CreateMasterTemplateResource("apiTemplate", initialAPIUri, initialAPIDependsOn));

            masterTemplate.resources = resources.ToArray();
            return masterTemplate;
        }

        public Template CreateUnlinkedMasterTemplate(Template apiVersionSetTemplate,
            Template apiTemplate,
            CreatorFileNames creatorFileNames)
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
            resources.AddRange(apiTemplate.resources);

            masterTemplate.resources = resources.ToArray();
            return masterTemplate;
        }

        public MasterTemplateResource CreateMasterTemplateResource(string name, string uriLink, string[] dependsOn)
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

            // add parameters
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
