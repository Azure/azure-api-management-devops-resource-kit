using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class APIVersionSetTemplateCreator : TemplateGeneratorBase
    {
        public Template CreateAPIVersionSetTemplate(CreatorConfig creatorConfig)
        {
            // create empty template
            Template apiVersionSetTemplate = this.GenerateEmptyTemplate();

            // add parameters
            apiVersionSetTemplate.Parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ type = "string" } }
            };

            List<TemplateResource> resources = new List<TemplateResource>();
            foreach (APIVersionSetConfig apiVersionSet in creatorConfig.apiVersionSets)
            {
                // create apiVersionSet resource with properties
                // default version set id to version set if id is not provided
                string versionSetId = apiVersionSet != null && apiVersionSet.id != null ? apiVersionSet.id : "versionset";
                APIVersionSetTemplateResource apiVersionSetTemplateResource = new APIVersionSetTemplateResource()
                {
                    Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{versionSetId}')]",
                    Type = ResourceTypeConstants.APIVersionSet,
                    ApiVersion = GlobalConstants.ApiVersion,
                    Properties = new APIVersionSetProperties()
                    {
                        displayName = apiVersionSet.displayName,
                        description = apiVersionSet.description,
                        versionHeaderName = apiVersionSet.versionHeaderName,
                        versionQueryName = apiVersionSet.versionQueryName,
                        versioningScheme = apiVersionSet.versioningScheme,
                    },
                    DependsOn = new string[] { }
                };
                resources.Add(apiVersionSetTemplateResource);
            }

            apiVersionSetTemplate.Resources = resources.ToArray();
            return apiVersionSetTemplate;
        }
    }
}
