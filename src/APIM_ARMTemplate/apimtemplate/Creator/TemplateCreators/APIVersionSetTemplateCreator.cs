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
        public APIVersionSetTemplate CreateAPIVersionSetTemplate(CreatorConfig creatorConfig)
        {
            // create api schema with properties
            APIVersionSetTemplate apiVersionSetTemplate = new APIVersionSetTemplate()
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
            return apiVersionSetTemplate;
        }
    }
}
