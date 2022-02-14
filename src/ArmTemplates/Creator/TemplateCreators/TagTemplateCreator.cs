using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class TagTemplateCreator : TemplateCreator
    {
        public Template CreateTagTemplate(CreatorConfig creatorConfig)
        {
            // create empty template
            Template tagTemplate = CreateEmptyTemplate();

            // add parameters
            tagTemplate.parameters = new Dictionary<string, TemplateParameterProperties>
            {
                {ParameterNames.ApimServiceName, new TemplateParameterProperties(){ type = "string" }}
            };

            // aggregate all tags from apis
            HashSet<string> tagHashset = new HashSet<string>();
            List<APIConfig> apis = creatorConfig.apis;
            if (apis != null)
            {
                foreach (APIConfig api in apis)
                {
                    if (api.tags != null)
                    {
                        string[] apiTags = api.tags.Split(", ");
                        foreach (string apiTag in apiTags)
                        {
                            tagHashset.Add(apiTag);
                        }
                    }
                }
            }
            foreach (TagTemplateProperties tag in creatorConfig.tags)
            {
                tagHashset.Add(tag.displayName);
            }

            List<TemplateResource> resources = new List<TemplateResource>();
            foreach (string tag in tagHashset)
            {
                // create tag resource with properties
                TagTemplateResource tagTemplateResource = new TagTemplateResource()
                {
                    name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{tag}')]",
                    type = ResourceTypeConstants.Tag,
                    apiVersion = GlobalConstants.APIVersion,
                    properties = new TagTemplateProperties()
                    {
                        displayName = tag
                    },
                    dependsOn = new string[] { }
                };
                resources.Add(tagTemplateResource);
            }

            tagTemplate.resources = resources.ToArray();
            return tagTemplate;
        }
    }
}