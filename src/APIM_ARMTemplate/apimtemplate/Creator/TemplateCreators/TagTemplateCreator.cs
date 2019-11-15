using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

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

            List<TemplateResource> resources = new List<TemplateResource>();
            foreach (TagTemplateProperties tag in creatorConfig.tags)
            {
                // create tag resource with properties
                TagTemplateResource tagTemplateResource = new TagTemplateResource()
                {
                    name = $"[concat(parameters('ApimServiceName'), '/{tag.displayName}')]",
                    type = ResourceTypeConstants.Tag,
                    apiVersion = GlobalConstants.APIVersion,
                    properties = new TagTemplateProperties()
                    {
                        displayName = tag.displayName
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