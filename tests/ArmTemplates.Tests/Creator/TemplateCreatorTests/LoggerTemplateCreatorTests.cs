// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

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
                LoggerType = "applicationinsights",
                Description = "description",
                IsBuffered = true,
                ResourceId = "resourceId",
                Credentials = new LoggerCredentials()
                {
                    ConnectionString = "connString",
                    InstrumentationKey = "iKey",
                    Name = "credName"
                }

            };
            creatorConfig.loggers.Add(logger);

            // act
            Template loggerTemplate = loggerTemplateCreator.CreateLoggerTemplate(creatorConfig);
            LoggerTemplateResource loggerTemplateResource = (LoggerTemplateResource)loggerTemplate.Resources[0];

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{logger.name}')]", loggerTemplateResource.Name);
            Assert.Equal(logger.LoggerType, loggerTemplateResource.Properties.LoggerType);
            Assert.Equal(logger.Description, loggerTemplateResource.Properties.Description);
            Assert.Equal(logger.IsBuffered, loggerTemplateResource.Properties.IsBuffered);
            Assert.Equal(logger.ResourceId, loggerTemplateResource.Properties.ResourceId);
            Assert.Equal(logger.Credentials.ConnectionString, loggerTemplateResource.Properties.Credentials.ConnectionString);
            Assert.Equal(logger.Credentials.InstrumentationKey, loggerTemplateResource.Properties.Credentials.InstrumentationKey);
            Assert.Equal(logger.Credentials.Name, loggerTemplateResource.Properties.Credentials.Name);
        }
    }
}
