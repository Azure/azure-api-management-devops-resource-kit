using System.Collections.Generic;

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
            // create apiVersionSet resource with properties
            string versionSetId = (creatorConfig.apiVersionSet != null && creatorConfig.apiVersionSet.id != null) ? creatorConfig.apiVersionSet.id : "versionset";
            APIVersionSetTemplateResource apiVersionSetTemplateResource = new APIVersionSetTemplateResource()
            {
                name = $"[concat(parameters('ApimServiceName'), '/{versionSetId}')]",
                type = "Microsoft.ApiManagement/service/api-version-sets",
                apiVersion = "2018-01-01",
                properties = new APIVersionSetProperties()
                {
                    displayName = creatorConfig.apiVersionSet.displayName,
                    description = creatorConfig.apiVersionSet.description,
                    versionHeaderName = creatorConfig.apiVersionSet.versionHeaderName,
                    versionQueryName = creatorConfig.apiVersionSet.versionQueryName,
                    versioningScheme = creatorConfig.apiVersionSet.versioningScheme,
                },
                dependsOn = new string[] { }
            };
            resources.Add(apiVersionSetTemplateResource);

            apiVersionSetTemplate.resources = resources.ToArray();
            return apiVersionSetTemplate;
        }
    }
}
