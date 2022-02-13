using System.Collections.Generic;
using apimtemplate.Common.Constants;
using apimtemplate.Common.TemplateModels;
using apimtemplate.Common.Templates.Abstractions;
using apimtemplate.Creator.Models;

namespace apimtemplate.Creator.TemplateCreators
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
            foreach (BackendTemplateProperties backendTemplatePropeties in creatorConfig.backends)
            {
                // create backend resource with properties
                BackendTemplateResource backendTemplateResource = new BackendTemplateResource()
                {
                    name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{backendTemplatePropeties.title}')]",
                    type = ResourceTypeConstants.Backend,
                    apiVersion = GlobalConstants.APIVersion,
                    properties = backendTemplatePropeties,
                    dependsOn = new string[] { }
                };
                resources.Add(backendTemplateResource);
            }

            backendTemplate.resources = resources.ToArray();
            return backendTemplate;
        }
    }
}
