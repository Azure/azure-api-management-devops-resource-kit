using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
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
            Template apiVersionSetTemplate = this.templateCreator.CreateEmptyTemplate();

            // add parameters
            apiVersionSetTemplate.parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { "ApimServiceName", new TemplateParameterProperties(){ type = "string" } }
            };

            List<TemplateResource> resources = new List<TemplateResource>();
            // create apiVersionSet resource with properties
            APIVersionSetTemplateResource apiVersionSetTemplateResource = new APIVersionSetTemplateResource()
            {
                name = "[concat(parameters('ApimServiceName'), '/versionset')]",
                type = "Microsoft.ApiManagement/service/api-version-sets",
                apiVersion = "2018-06-01-preview",
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
