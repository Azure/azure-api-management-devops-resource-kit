using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class TagTemplateCreator: TemplateCreator
    {
        public Template CreateTagTemplate(CreatorConfig creatorConfig)
        {
            // create empty template
            Template tagTemplate = CreateEmptyTemplate();

            // add parameters
            tagTemplate.parameters = new Dictionary<string, TemplateParameterProperties>
            {
                {"ApimServiceName", new TemplateParameterProperties(){ type = "string" }}
            };

            // aggregate all tags from apis
            HashSet<string> tagHashset = new HashSet<string>(); 
            List<APIConfig> apis = creatorConfig.apis;
            foreach(APIConfig api in apis) 
            {
                string[] apiTags = api.tags.Split(", ");
                foreach(string apiTag in apiTags) {
                    tagHashset.Add(apiTag);
                }
            }
            foreach(TagTemplateProperties tag in creatorConfig.tags)
            {
                tagHashset.Add(tag.displayName);
            }
            
            List<TemplateResource> resources = new List<TemplateResource>();
            foreach (string tag in tagHashset)
            {
                // create tag resource with properties
                TagTemplateResource tagTemplateResource = new TagTemplateResource()
                {
                    name = $"[concat(parameters('ApimServiceName'), '/{tag}')]",
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