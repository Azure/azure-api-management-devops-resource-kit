// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.TagApi;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class TagApiTemplateCreator : ITagApiTemplateCreator
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

        public List<TagApiTemplateResource> CreateTagAPITemplateResources(ApiConfig api, string[] dependsOn)
        {
            // create a tag/apis association resource for each tag in the config file
            List<TagApiTemplateResource> tagAPITemplates = new List<TagApiTemplateResource>();
            // tags is comma seperated list pf tags
            string[] tagIDs = api.Tags.Split(",", System.StringSplitOptions.TrimEntries);
            foreach (string tagID in tagIDs)
            {
                TagApiTemplateResource tagAPITemplate = this.CreateTagAPITemplateResource(tagID, api.Name, dependsOn);
                tagAPITemplates.Add(tagAPITemplate);
            }
            return tagAPITemplates;
        }
    }
}