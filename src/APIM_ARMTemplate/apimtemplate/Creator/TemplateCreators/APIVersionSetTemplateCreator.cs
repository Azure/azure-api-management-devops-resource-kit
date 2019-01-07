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
        public Template CreateAPIVersionSetTemplate(CreatorConfig creatorConfig)
        {
            Template apiVersionSetTemplate = new Template()
            {
                schema = "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
                contentVersion = "1.0.0.0",
                parameters = { },
                variables = { },
                resources = new TemplateResource[] { },
                outputs = { }
            };

            List<TemplateResource> resources = new List<TemplateResource>();
            // create apiVersionSet schema with properties
            APIVersionSetTemplateResource apiVersionSetTemplateResource = new APIVersionSetTemplateResource()
            {
                type = "Microsoft.ApiManagement/service/api-version-sets",
                apiVersion = "2018-06-01-preview",
                properties = new APIVersionSetProperties()
                {
                    displayName = creatorConfig.apiVersionSet.displayName,
                    description = creatorConfig.apiVersionSet.description,
                    versionHeaderName = creatorConfig.apiVersionSet.versionHeaderName,
                    versionQueryName = creatorConfig.apiVersionSet.versionQueryName,
                    versioningScheme = creatorConfig.apiVersionSet.versioningScheme,
                }
            };
            resources.Add(apiVersionSetTemplateResource);

            apiVersionSetTemplate.resources = resources.ToArray();
            return apiVersionSetTemplate;
        }
    }
}
