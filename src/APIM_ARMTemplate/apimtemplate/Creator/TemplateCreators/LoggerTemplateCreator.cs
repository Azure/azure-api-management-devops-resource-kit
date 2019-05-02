using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class LoggerTemplateCreator : TemplateCreator
    {
        public Template CreateLoggerTemplate(CreatorConfig creatorConfig)
        {
            // create empty template
            Template loggerTemplate = CreateEmptyTemplate();

            // add parameters
            loggerTemplate.parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { "ApimServiceName", new TemplateParameterProperties(){ type = "string" } }
            };

            List<TemplateResource> resources = new List<TemplateResource>();
            foreach (LoggerConfig logger in creatorConfig.loggers)
            {
                // create logger resource with properties
                LoggerTemplateResource loggerTemplateResource = new LoggerTemplateResource()
                {
                    name = $"[concat(parameters('ApimServiceName'), '/{logger.name}')]",
                    type = ResourceTypeConstants.Logger,
                    apiVersion = GlobalConstants.APIVersion,
                    properties = new LoggerTemplateProperties()
                    {
                        loggerType = logger.loggerType,
                        description = logger.description,
                        credentials = logger.credentials,
                        isBuffered = logger.isBuffered,
                        resourceId = logger.resourceId
                    },
                    dependsOn = new string[] { }
                };
                resources.Add(loggerTemplateResource);
            }

            loggerTemplate.resources = resources.ToArray();
            return loggerTemplate;
        }
    }
}
