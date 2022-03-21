using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class LoggerTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateLoggerTemplateFromCreatorConfig()
        {
            // arrange
            LoggerTemplateCreator loggerTemplateCreator = new LoggerTemplateCreator(new TemplateBuilder());
            CreatorConfig creatorConfig = new CreatorConfig() { loggers = new List<LoggerConfig>() };
            LoggerConfig logger = new LoggerConfig()
            {
                name = "name",
                loggerType = "applicationinsights",
                description = "description",
                isBuffered = true,
                resourceId = "resourceId",
                credentials = new LoggerCredentials()
                {
                    connectionString = "connString",
                    instrumentationKey = "iKey",
                    name = "credName"
                }

            };
            creatorConfig.loggers.Add(logger);

            // act
            Template loggerTemplate = loggerTemplateCreator.CreateLoggerTemplate(creatorConfig);
            LoggerTemplateResource loggerTemplateResource = (LoggerTemplateResource)loggerTemplate.Resources[0];

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{logger.name}')]", loggerTemplateResource.Name);
            Assert.Equal(logger.loggerType, loggerTemplateResource.Properties.loggerType);
            Assert.Equal(logger.description, loggerTemplateResource.Properties.description);
            Assert.Equal(logger.isBuffered, loggerTemplateResource.Properties.isBuffered);
            Assert.Equal(logger.resourceId, loggerTemplateResource.Properties.resourceId);
            Assert.Equal(logger.credentials.connectionString, loggerTemplateResource.Properties.credentials.connectionString);
            Assert.Equal(logger.credentials.instrumentationKey, loggerTemplateResource.Properties.credentials.instrumentationKey);
            Assert.Equal(logger.credentials.name, loggerTemplateResource.Properties.credentials.name);
        }
    }
}
