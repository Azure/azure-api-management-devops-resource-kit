using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class TagAPITemplateCreator : TemplateGeneratorBase
    {
        public TagAPITemplateResource CreateTagAPITemplateResource(string tagName, string apiName, string[] dependsOn)
        {
            // create tags/apis resource with properties
            TagAPITemplateResource tagAPITemplateResource = new TagAPITemplateResource()
            {
                Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{tagName}')]",
                Type = ResourceTypeConstants.APITag,
                ApiVersion = GlobalConstants.ApiVersion,
                Properties = new TagAPITemplateProperties()
                {
                    displayName = tagName
                },
                DependsOn = dependsOn
            };
            return tagAPITemplateResource;
        }

        public List<TagAPITemplateResource> CreateTagAPITemplateResources(APIConfig api, string[] dependsOn)
        {
            // create a tag/apis association resource for each tag in the config file
            List<TagAPITemplateResource> tagAPITemplates = new List<TagAPITemplateResource>();
            // tags is comma seperated list pf tags
            string[] tagIDs = api.tags.Split(", ");
            foreach (string tagID in tagIDs)
            {
                TagAPITemplateResource tagAPITemplate = this.CreateTagAPITemplateResource(tagID, api.name, dependsOn);
                tagAPITemplates.Add(tagAPITemplate);
            }
            return tagAPITemplates;
        }
    }
}