using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class APIVersionSetTemplateCreator
    {
        private TemplateCreator templateCreator;

        public APIVersionSetTemplateCreator(TemplateCreator templateCreator)
        {
            this.templateCreator = templateCreator;
        }

        public Template CreateAPIVersionSetTemplate(CreatorConfig creatorConfig)
        {
            // create empty template
            Template apiVersionSetTemplate = this.templateCreator.CreateEmptyTemplate();

            // add parameters
            apiVersionSetTemplate.parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { "ApimServiceName", new TemplateParameterProperties(){ type = "string" } }
            };

            List<TemplateResource> resources = new List<TemplateResource>();
            foreach(APIVersionSetConfig apiVersionSet in creatorConfig.apiVersionSets)
            {
                // create apiVersionSet resource with properties
                string versionSetId = (apiVersionSet != null && apiVersionSet.id != null) ? apiVersionSet.id : "versionset";
                APIVersionSetTemplateResource apiVersionSetTemplateResource = new APIVersionSetTemplateResource()
                {
                    name = $"[concat(parameters('ApimServiceName'), '/{versionSetId}')]",
                    type = ResourceTypeConstants.APIVersionSet,
                    apiVersion = "2018-06-01-preview",
                    properties = new APIVersionSetProperties()
                    {
                        displayName = apiVersionSet.displayName,
                        description = apiVersionSet.description,
                        versionHeaderName = apiVersionSet.versionHeaderName,
                        versionQueryName = apiVersionSet.versionQueryName,
                        versioningScheme = apiVersionSet.versioningScheme,
                    },
                    dependsOn = new string[] { }
                };
                resources.Add(apiVersionSetTemplateResource);
            }

            apiVersionSetTemplate.resources = resources.ToArray();
            return apiVersionSetTemplate;
        }
    }
}
