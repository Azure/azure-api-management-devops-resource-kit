// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.TagApi;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class TagAPITemplateCreator
    {
        public TagApiTemplateResource CreateTagAPITemplateResource(string tagName, string apiName, string[] dependsOn)
        {
            // create tags/apis resource with properties
            TagApiTemplateResource tagAPITemplateResource = new TagApiTemplateResource()
            {
                Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{tagName}')]",
                Type = ResourceTypeConstants.APITag,
                ApiVersion = GlobalConstants.ApiVersion,
                Properties = new TagApiProperties()
                {
                    DisplayName = tagName
                },
                DependsOn = dependsOn
            };
            return tagAPITemplateResource;
        }

        public List<TagApiTemplateResource> CreateTagAPITemplateResources(APIConfig api, string[] dependsOn)
        {
            // create a tag/apis association resource for each tag in the config file
            List<TagApiTemplateResource> tagAPITemplates = new List<TagApiTemplateResource>();
            // tags is comma seperated list pf tags
            string[] tagIDs = api.tags.Split(", ");
            foreach (string tagID in tagIDs)
            {
                TagApiTemplateResource tagAPITemplate = this.CreateTagAPITemplateResource(tagID, api.name, dependsOn);
                tagAPITemplates.Add(tagAPITemplate);
            }
            return tagAPITemplates;
        }
    }
}