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
            Template initialAPITemplate,
            Template subsequentAPITemplate,
            List<Template> productAPITemplates,
            Template apiPolicyTemplate,
            List<Template> operationPolicyTemplates,
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

            //initial API
            string initialAPIUri = $"[concat(parameters('repoBaseUrl'), '{creatorFileNames.initialAPI}')]";
            string[] initialAPIDependsOn = apiVersionSetTemplate != null ? new string[] { "[resourceId('Microsoft.Resources/deployments', 'versionSetTemplate')]" } : new string[] { };
            resources.Add(this.CreateMasterTemplateResource("initialAPITemplate", initialAPIUri, initialAPIDependsOn));

            //subsequent API
            string subsequentAPIUri = $"[concat(parameters('repoBaseUrl'), '{creatorFileNames.subsequentAPI}')]";
            string[] subsequentAPIDependsOn = apiVersionSetTemplate != null ? new string[] { "[resourceId('Microsoft.Resources/deployments', 'initialAPITemplate')]" } : new string[] { };
            resources.Add(this.CreateMasterTemplateResource("subsequentAPITemplate", subsequentAPIUri, subsequentAPIDependsOn));

            // apiPolicy
            if (apiPolicyTemplate != null)
            {
                string apiPolicyUri = $"[concat(parameters('repoBaseUrl'), '{creatorFileNames.apiPolicy}')]";
                string[] apiPolicyDependsOn = apiVersionSetTemplate != null ? new string[] { "[resourceId('Microsoft.Resources/deployments', 'subsequentAPITemplate')]" } : new string[] { };
                resources.Add(this.CreateMasterTemplateResource("apiPolicyTemplate", apiPolicyUri, apiPolicyDependsOn));
            }

            // productAPIs
            if (productAPITemplates.Count > 0)
            {
                foreach (Template productAPITemplate in productAPITemplates)
                {
                    KeyValuePair<string, string> filePair = this.fileNameGenerator.RetrieveFileNameForProductAPITemplate(productAPITemplate, creatorConfig);
                    string productAPIName = $"productAPI-{filePair.Key}Template";
                    string productAPIUri = $"[concat(parameters('repoBaseUrl'), '{filePair.Value}')]";
                    string[] productAPIDependsOn = apiVersionSetTemplate != null ? new string[] { "[resourceId('Microsoft.Resources/deployments', 'subsequentAPITemplate')]" } : new string[] { };
                    resources.Add(this.CreateMasterTemplateResource(productAPIName, productAPIUri, productAPIDependsOn));
                }
            }

            // operationPolicies
            if (operationPolicyTemplates.Count > 0)
            {
                foreach (Template operationPolicyTemplate in operationPolicyTemplates)
                {
                    KeyValuePair<string, string> filePair = this.fileNameGenerator.RetrieveFileNameForOperationPolicyTemplate(operationPolicyTemplate, creatorConfig);
                    string operationPolicyName = $"operationPolicy-{filePair.Key}Template";
                    string operationPolicyUri = $"[concat(parameters('repoBaseUrl'), '{filePair.Value}')]";
                    string[] operationPolicyDependsOn = apiVersionSetTemplate != null ? new string[] { "[resourceId('Microsoft.Resources/deployments', 'subsequentAPITemplate')]" } : new string[] { };
                    resources.Add(this.CreateMasterTemplateResource(operationPolicyName, operationPolicyUri, operationPolicyDependsOn));
                }
            }

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
                }
            };
            parameters.Add("repoBaseUrl", repoBaseUrlProperties);
            TemplateParameterProperties apimServiceNameProperties = new TemplateParameterProperties()
            {
                value = creatorConfig.apimServiceName,
                metadata = new TemplateParameterMetadata()
                {
                    description = "Name of the API Management"
                }
            };
            parameters.Add("ApimServiceName", apimServiceNameProperties);
            return parameters;
        }
    }
}
