using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class TagAPITemplateCreator
    {
        public TagAPITemplateResource CreateTagAPITemplateResource(string tagName, string apiName, string[] dependsOn)
        {
            // create tags/apis resource with properties
            TagAPITemplateResource tagAPITemplateResource = new TagAPITemplateResource(){
                name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{tagName}')]",
                type = ResourceTypeConstants.APITag,
                apiVersion = GlobalConstants.APIVersion,
                properties = new TagAPITemplateProperties(){
                    displayName = tagName
                },
                dependsOn = dependsOn
            };
            return tagAPITemplateResource;
        }

        public List<TagAPITemplateResource> CreateTagAPITemplateResources(APIConfig api, string[] dependsOn)
        {
            // create a tag/apis association resource for each tag in the config file
            List<TagAPITemplateResource> tagAPITemplates = new List<TagAPITemplateResource>();
            // tags is comma seperated list pf tags
            string[] tagIDs = api.tags.Split(", ");
            foreach(string tagID in tagIDs) 
            {
                TagAPITemplateResource tagAPITemplate = this.CreateTagAPITemplateResource(tagID, api.name, dependsOn);
                tagAPITemplates.Add(tagAPITemplate);
            }
            return tagAPITemplates;
        }
    }
}