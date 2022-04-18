// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class LoggerTemplateCreator
    {
        readonly ITemplateBuilder templateBuilder;

        public LoggerTemplateCreator(ITemplateBuilder templateBuilder)
        {
            this.templateBuilder = templateBuilder;
        }

        public Template CreateLoggerTemplate(CreatorConfig creatorConfig)
        {
            // create empty template
            Template loggerTemplate = this.templateBuilder.GenerateEmptyTemplate().Build();

            // add parameters
            loggerTemplate.Parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ Type = "string" } }
            };

            List<TemplateResource> resources = new List<TemplateResource>();
            foreach (LoggerConfig logger in creatorConfig.loggers)
            {
                // create logger resource with properties
                LoggerTemplateResource loggerTemplateResource = new LoggerTemplateResource()
                {
                    Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{logger.name}')]",
                    Type = ResourceTypeConstants.Logger,
                    ApiVersion = GlobalConstants.ApiVersion,
                    Properties = new LoggerTemplateProperties()
                    {
                        LoggerType = logger.LoggerType,
                        Description = logger.Description,
                        Credentials = logger.Credentials,
                        IsBuffered = logger.IsBuffered,
                        ResourceId = logger.ResourceId
                    },
                    DependsOn = new string[] { }
                };
                resources.Add(loggerTemplateResource);
            }

            loggerTemplate.Resources = resources.ToArray();
            return loggerTemplate;
        }
    }
}
