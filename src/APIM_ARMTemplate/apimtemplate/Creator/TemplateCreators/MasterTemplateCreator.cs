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
        private FileNameGenerator fileNameGenerator;

        public MasterTemplateCreator(TemplateCreator templateCreator, FileNameGenerator fileNameGenerator)
        {
            this.templateCreator = templateCreator;
            this.fileNameGenerator = fileNameGenerator;
        }

        public Template CreateLinkedMasterTemplate(CreatorConfig creatorConfig,
            Template apiVersionSetTemplate,
            Template apiTemplate,
            CreatorFileNames creatorFileNames)
        {
            Template masterTemplate = this.templateCreator.CreateEmptyTemplate();

            // add parameters
            masterTemplate.parameters = this.CreateMasterTemplateParameters(creatorConfig);

            // add links to all resources
            List<TemplateResource> resources = new List<TemplateResource>();

            // apiVersionSet
            if (apiVersionSetTemplate != null)
            {
                string apiVersionSetUri = $"[concat(parameters('repoBaseUrl'), '{creatorFileNames.apiVersionSet}')]";
                resources.Add(this.CreateMasterTemplateResource("versionSetTemplate", apiVersionSetUri, new string[] { }));
            }

            //api
            string initialAPIUri = $"[concat(parameters('repoBaseUrl'), '{creatorFileNames.api}')]";
            string[] initialAPIDependsOn = apiVersionSetTemplate != null ? new string[] { "[resourceId('Microsoft.Resources/deployments', 'versionSetTemplate')]" } : new string[] { };
            resources.Add(this.CreateMasterTemplateResource("apiTemplate", initialAPIUri, initialAPIDependsOn));

            masterTemplate.resources = resources.ToArray();
            return masterTemplate;
        }

        public MasterTemplateResource CreateMasterTemplateResource(string name, string uriLink, string[] dependsOn)
        {
            MasterTemplateResource masterTemplateResource = new MasterTemplateResource()
            {
                name = name,
                type = "Microsoft.Resources/deployments",
                apiVersion = "2018-06-01-preview",
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

        public Dictionary<string, TemplateParameterProperties> CreateMasterTemplateParameters(CreatorConfig creatorConfig)
        {
            Dictionary<string, TemplateParameterProperties> parameters = new Dictionary<string, TemplateParameterProperties>();
            TemplateParameterProperties repoBaseUrlProperties = new TemplateParameterProperties()
            {
                value = ".",
                metadata = new TemplateParameterMetadata()
                {
                    description = "Base URL of the repository"
                },
                type = "string"
            };
            parameters.Add("repoBaseUrl", repoBaseUrlProperties);
            TemplateParameterProperties apimServiceNameProperties = new TemplateParameterProperties()
            {
                value = creatorConfig.apimServiceName,
                metadata = new TemplateParameterMetadata()
                {
                    description = "Name of the API Management"
                },
                type = "string"
            };
            parameters.Add("ApimServiceName", apimServiceNameProperties);
            return parameters;
        }
    }
}
