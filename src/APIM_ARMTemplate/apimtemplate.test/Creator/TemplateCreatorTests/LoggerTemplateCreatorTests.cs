﻿using Xunit;
using System.Collections.Generic;
using apimtemplate.Common.Templates.Abstractions;
using apimtemplate.Creator.Models;
using apimtemplate.Common.Templates.Logger;
using apimtemplate.Creator.TemplateCreators;

namespace apimtemplate.test.Creator.TemplateCreatorTests
{
    public class LoggerTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateLoggerTemplateFromCreatorConfig()
        {
            // arrange
            LoggerTemplateCreator loggerTemplateCreator = new LoggerTemplateCreator();
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
            LoggerTemplateResource loggerTemplateResource = (LoggerTemplateResource)loggerTemplate.resources[0];

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{logger.name}')]", loggerTemplateResource.name);
            Assert.Equal(logger.loggerType, loggerTemplateResource.properties.loggerType);
            Assert.Equal(logger.description, loggerTemplateResource.properties.description);
            Assert.Equal(logger.isBuffered, loggerTemplateResource.properties.isBuffered);
            Assert.Equal(logger.resourceId, loggerTemplateResource.properties.resourceId);
            Assert.Equal(logger.credentials.connectionString, loggerTemplateResource.properties.credentials.connectionString);
            Assert.Equal(logger.credentials.instrumentationKey, loggerTemplateResource.properties.credentials.instrumentationKey);
            Assert.Equal(logger.credentials.name, loggerTemplateResource.properties.credentials.name);
        }
    }
}
