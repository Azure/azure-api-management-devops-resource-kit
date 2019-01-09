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

        public Dictionary<string, TemplateParameterProperties> CreateMasterTemplateParameters()
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
            return parameters;
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
                    }
                },
                dependsOn = dependsOn
            };
            return masterTemplateResource;
        }

        public Template CreateLinkedMasterTemplate(Template apiVersionSetTemplate,
            Template initialAPITemplate,
            Template subsequentAPITemplate,
            List<Template> productAPITemplates,
            Template apiPolicyTemplate,
            List<Template> operationPolicyTemplates,
            CreatorFileNames creatorFileNames)
        {
            Template masterTemplate = this.templateCreator.CreateEmptyTemplate();

            // add parameters
            masterTemplate.parameters = this.CreateMasterTemplateParameters();

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
            string subsequentAPIUri = $"[concat(parameters('repoBaseUrl'), '{creatorFileNames.initialAPI}')]";
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
                    string productAPIName = $"productAPI-{productAPITemplate.resources[0].name.Split("-")[1]}Template";
                    string productAPIUri = $"[concat(parameters('repoBaseUrl'), '{creatorFileNames.productAPIs.GetValueOrDefault(productAPITemplate.resources[0].name)}')]";
                    string[] productAPIDependsOn = apiVersionSetTemplate != null ? new string[] { "[resourceId('Microsoft.Resources/deployments', 'subsequentAPITemplate')]" } : new string[] { };
                    resources.Add(this.CreateMasterTemplateResource(productAPIName, productAPIUri, productAPIDependsOn));
                }
            }

            // operationPolicies
            if (operationPolicyTemplates.Count > 0)
            {
                foreach (Template operationPolicyTemplate in operationPolicyTemplates)
                {
                    string operationPolicyName = $"operationPolicy-{operationPolicyTemplate.resources[0].name.Split("-")[1]}Template";
                    string operationPolicyUri = $"[concat(parameters('repoBaseUrl'), '{creatorFileNames.productAPIs.GetValueOrDefault(operationPolicyTemplate.resources[0].name)}')]";
                    string[] operationPolicyDependsOn = apiVersionSetTemplate != null ? new string[] { "[resourceId('Microsoft.Resources/deployments', 'subsequentAPITemplate')]" } : new string[] { };
                    resources.Add(this.CreateMasterTemplateResource(operationPolicyName, operationPolicyUri, operationPolicyDependsOn));
                }
            }

            masterTemplate.resources = resources.ToArray();
            return masterTemplate;
        }
    }
}
