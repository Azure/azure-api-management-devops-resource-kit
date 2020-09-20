using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class BackendTemplateCreator : TemplateCreator
    {
        public Template CreateBackendTemplate(CreatorConfig creatorConfig)
        {
            // create empty template
            Template backendTemplate = CreateEmptyTemplate();

            // add parameters
            backendTemplate.parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ type = "string" } }
            };

            List<TemplateResource> resources = new List<TemplateResource>();
            StringBuilder serviceParameter = new StringBuilder();
            foreach (BackendTemplateProperties backendTemplatePropeties in creatorConfig.backends)
            {
                if (serviceParameter.Length == 0) serviceParameter.Append("{");
                else serviceParameter.Append(",");
                serviceParameter.Append(string.Concat("\"", GetSanitisedName(backendTemplatePropeties.title), "\": \"", backendTemplatePropeties.url, "\""));

                // create backend resource with properties
                BackendTemplateResource backendTemplateResource = new BackendTemplateResource()
                {
                    name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{backendTemplatePropeties.title}')]",
                    type = ResourceTypeConstants.Backend,
                    apiVersion = GlobalConstants.APIVersion,
                    properties = backendTemplatePropeties,
                    dependsOn = new string[] { }
                };
                backendTemplateResource.properties.url = $"[parameters('{ParameterNames.ServiceUrl}').{GetSanitisedName(backendTemplatePropeties.title)}]";
                resources.Add(backendTemplateResource);
            }

            serviceParameter.Append("}");
            backendTemplate.parameters.Add(ParameterNames.ServiceUrl, new TemplateParameterProperties() { type = "object", defaultValue = serviceParameter.ToString() });

            backendTemplate.resources = resources.ToArray();
            return backendTemplate;
        }

        private string GetSanitisedName(string name)
        {
            return name.Replace("-", "");
        }
    }
}