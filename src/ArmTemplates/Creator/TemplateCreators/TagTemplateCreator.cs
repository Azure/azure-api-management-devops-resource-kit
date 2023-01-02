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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class TagTemplateCreator : ITagTemplateCreator
    {
        readonly ITemplateBuilder templateBuilder;

        public TagTemplateCreator(ITemplateBuilder templateBuilder)
        {
            this.templateBuilder = templateBuilder;
        }

        static void AddTagNameToDictionary(string tagName, Dictionary<string, string> tagsDictionary)
        {
            var resourceName = NamingHelper.GenerateValidResourceNameFromDisplayName(tagName);

            if (string.IsNullOrEmpty(resourceName)) 
            {
                throw new EmptyResourceNameException(tagName);
            }

            if (tagsDictionary.ContainsKey(resourceName))
            {
                var existingValue = tagsDictionary[resourceName];
                if (!existingValue.Equals(tagName))
                {
                    throw new DuplicateTagResourceNameException(existingValue, tagName, resourceName);
                }
            }
            else
            {
                tagsDictionary.Add(resourceName, tagName);
            }
        }

        public Template CreateTagTemplate(CreatorParameters creatorConfig)
        {
            var tagTemplate = this.templateBuilder.GenerateTemplateWithApimServiceNameProperty().Build();
            var tagsDictionary = new Dictionary<string, string>();

            if (!creatorConfig.Apis.IsNullOrEmpty())
            {
                foreach (var api in creatorConfig.Apis)
                {
                    if (!api.Tags.IsNullOrEmpty())
                    {
                        var apiTags = api.Tags.Split(",", System.StringSplitOptions.TrimEntries);
                        
                        foreach (var apiTag in apiTags)
                        {
                            AddTagNameToDictionary(apiTag, tagsDictionary);
                        }
                    }
                }
            }

            if (!creatorConfig.Tags.IsNullOrEmpty())
            {
                foreach (var tag in creatorConfig.Tags)
                {
                    AddTagNameToDictionary(tag.DisplayName, tagsDictionary);
                }
            }

            var resources = new List<TemplateResource>();

            foreach (var (tagResourceName, tagDisplayName) in tagsDictionary)
            {
                var tagTemplateResource = new TagTemplateResource()
                {
                    Name = NamingHelper.GenerateParametrizedResourceName(ParameterNames.ApimServiceName, tagResourceName),
                    Type = ResourceTypeConstants.Tag,
                    ApiVersion = GlobalConstants.ApiVersion,
                    Properties = new TagProperties()
                    {
                        DisplayName = tagDisplayName
                    }
                };
                resources.Add(tagTemplateResource);
            }

            tagTemplate.Resources = resources.ToArray();
            return tagTemplate;
        }
    }
}