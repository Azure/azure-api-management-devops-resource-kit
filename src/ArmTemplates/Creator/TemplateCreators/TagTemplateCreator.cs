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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;

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
            var tagTemplate = this.templateBuilder.GenerateTemplateWithApimServiceNameProperty().Build();
            var tagHashset = new HashSet<string>();

            if (!creatorConfig.Apis.IsNullOrEmpty())
            {
                foreach (var api in creatorConfig.Apis)
                {
                    if (api.Tags != null)
                    {
                        var apiTags = api.Tags.Split(", ");
                        
                        foreach (var apiTag in apiTags)
                        {
                            tagHashset.Add(apiTag);
                        }
                    }
                }
            }

            if (!creatorConfig.Tags.IsNullOrEmpty())
            {
                foreach (var tag in creatorConfig.Tags)
                {
                    tagHashset.Add(tag.DisplayName);
                }
            }

            var resources = new List<TemplateResource>();

            foreach (var tag in tagHashset)
            {
                var tagTemplateResource = new TagTemplateResource()
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