// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class TagTemplateCreator : ITagTemplateCreator
    {
        readonly ITemplateBuilder templateBuilder;

        public TagTemplateCreator(ITemplateBuilder templateBuilder)
        {
            this.templateBuilder = templateBuilder;
        }

        public Template CreateTagTemplate(CreatorParameters creatorConfig)
        {
            // create empty template
            Template tagTemplate = this.templateBuilder.GenerateEmptyTemplate().Build();

            // add parameters
            tagTemplate.Parameters = new Dictionary<string, TemplateParameterProperties>
            {
                {ParameterNames.ApimServiceName, new TemplateParameterProperties(){ Type = "string" }}
            };

            // aggregate all tags from apis
            HashSet<string> tagHashset = new HashSet<string>();
            List<ApiConfig> apis = creatorConfig.Apis;
            if (apis != null)
            {
                foreach (ApiConfig api in apis)
                {
                    if (api.Tags != null)
                    {
                        string[] apiTags = api.Tags.Split(", ");
                        foreach (string apiTag in apiTags)
                        {
                            tagHashset.Add(apiTag);
                        }
                    }
                }
            }
            foreach (TagProperties tag in creatorConfig.Tags)
            {
                tagHashset.Add(tag.DisplayName);
            }

            List<TemplateResource> resources = new List<TemplateResource>();
            foreach (string tag in tagHashset)
            {
                // create tag resource with properties
                TagTemplateResource tagTemplateResource = new TagTemplateResource()
                {
                    Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{tag}')]",
                    Type = ResourceTypeConstants.Tag,
                    ApiVersion = GlobalConstants.ApiVersion,
                    Properties = new TagProperties()
                    {
                        DisplayName = tag
                    },
                    DependsOn = new string[] { }
                };
                resources.Add(tagTemplateResource);
            }

            tagTemplate.Resources = resources.ToArray();
            return tagTemplate;
        }
    }
}