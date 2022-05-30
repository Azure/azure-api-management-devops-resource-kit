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

        public Template CreateTagTemplate(CreatorParameters creatorConfig)
        {
            var tagTemplate = this.templateBuilder.GenerateTemplateWithApimServiceNameProperty().Build();
            var tagsDictionary = new Dictionary<string, string>();

            void AddTagNameToDictionary(string tagName, Dictionary<string, string> tagsDictionary)
            {
                var resourceName = ResourceNamingHelper.GenerateValidResourceNameFromDisplayName(tagName);

                if (tagsDictionary.ContainsKey(resourceName))
                {
                    var existingValue = tagsDictionary[resourceName];
                    if (!existingValue.Equals(tagName))
                    {
                        throw new DuplicateTagResourceNameException(string.Format(ErrorMessages.DuplicateTagResourceNameErrorMessage, existingValue, tagName, resourceName));
                    }
                }
                else
                {
                    tagsDictionary.Add(resourceName, tagName);
                }
            }

            if (!creatorConfig.Apis.IsNullOrEmpty())
            {
                foreach (var api in creatorConfig.Apis)
                {
                    if (!api.Tags.IsNullOrEmpty())
                    {
                        var apiTags = api.Tags.Split(", ");
                        
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
                    Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{tagResourceName}')]",
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