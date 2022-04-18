// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    public class MockLoggerClient
    {
        public const string LoggerName = "logger-name";

        public static ILoggerClient GetMockedClientWithDiagnosticDependentValues()
        {
            var mockLoggerClient = new Mock<ILoggerClient>(MockBehavior.Strict);

            mockLoggerClient
                .Setup(x => x.GetAllAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((ExtractorParameters _) => new List<LoggerTemplateResource>
                {
                    new LoggerTemplateResource
                    {
                        Name = LoggerName,
                        Type = ResourceTypeConstants.Logger,
                        Properties = new()
                        {
                            LoggerType = MockDiagnosticClient.DefaultDiagnosticName,
                            IsBuffered = true 
                        }
                    }
                });

            return mockLoggerClient.Object;
        }
    }
}
