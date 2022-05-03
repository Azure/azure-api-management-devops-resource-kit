// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiVersionSet;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class ApiVersionSetTemplateCreator : IApiVersionSetTemplateCreator
    {
        readonly ITemplateBuilder templateBuilder;

        public ApiVersionSetTemplateCreator(ITemplateBuilder templateBuilder)
        {
            this.templateBuilder = templateBuilder;
        }

        public Template CreateAPIVersionSetTemplate(CreatorParameters creatorConfig)
        {
            // create empty template
            Template apiVersionSetTemplate = this.templateBuilder.GenerateEmptyTemplate().Build();

            // add parameters
            apiVersionSetTemplate.Parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ Type = "string" } }
            };

            List<TemplateResource> resources = new List<TemplateResource>();
            foreach (ApiVersionSetConfig apiVersionSet in creatorConfig.ApiVersionSets)
            {
                // create apiVersionSet resource with properties
                // default version set id to version set if id is not provided
                string versionSetId = apiVersionSet != null && apiVersionSet.Id != null ? apiVersionSet.Id : "versionset";
                var apiVersionSetTemplateResource = new ApiVersionSetTemplateResource()
                {
                    Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{versionSetId}')]",
                    Type = ResourceTypeConstants.ApiVersionSet,
                    ApiVersion = GlobalConstants.ApiVersion,
                    Properties = new ApiVersionSetProperties()
                    {
                        DisplayName = apiVersionSet.DisplayName,
                        Description = apiVersionSet.Description,
                        VersionHeaderName = apiVersionSet.VersionHeaderName,
                        VersionQueryName = apiVersionSet.VersionQueryName,
                        VersioningScheme = apiVersionSet.VersioningScheme,
                    },
                    DependsOn = new string[] { }
                };
                resources.Add(apiVersionSetTemplateResource);
            }

            apiVersionSetTemplate.Resources = resources.ToArray();
            return apiVersionSetTemplate;
        }
    }
}
