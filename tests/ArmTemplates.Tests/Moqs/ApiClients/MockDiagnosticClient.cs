// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    public class MockDiagnosticClient
    {
        public const string DiagnosticName = "diagnostic";
        public const string DefaultDiagnosticName = "default-diagnostic";
        public const string LoggerIdValue = "logger-id";

        public static IDiagnosticClient GetMockedClientWithApiDependentValues()
        {
            var mockDiagnosticClient = new Mock<IDiagnosticClient>(MockBehavior.Strict);

            mockDiagnosticClient
                .Setup(x => x.GetAllAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((ExtractorParameters _) => new List<DiagnosticTemplateResource>
                {
                    new DiagnosticTemplateResource
                    {
                        OriginalName = DiagnosticName,
                        Name = DiagnosticName,
                        Properties = new DiagnosticTemplateProperties()
                        {
                            LoggerId = LoggerIdValue
                        }
                    }
                });

            mockDiagnosticClient
                .Setup(x => x.GetApiDiagnosticsAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((string apiName, ExtractorParameters _) => new List<DiagnosticTemplateResource>
                {
                    new DiagnosticTemplateResource
                    {
                        OriginalName= $"{apiName}-{DiagnosticName}",
                        Name = $"{apiName}-{DiagnosticName}",
                        Properties = new DiagnosticTemplateProperties()
                        {
                            LoggerId = LoggerIdValue
                        }
                    }
                });

            return mockDiagnosticClient.Object;
        }

        public static IDiagnosticClient GetMockedApiClientWithDefaultValues()
        {
            var mockDiagnosticClient = new Mock<IDiagnosticClient>(MockBehavior.Strict);

            mockDiagnosticClient
                .Setup(x => x.GetAllAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((ExtractorParameters _) => new List<DiagnosticTemplateResource>
                {
                    new DiagnosticTemplateResource
                    {
                        OriginalName = DefaultDiagnosticName,
                        Name = DefaultDiagnosticName,
                        Properties = new DiagnosticTemplateProperties()
                        {
                            LoggerId = LoggerIdValue
                        }
                    }
                });

            mockDiagnosticClient
                .Setup(x => x.GetApiDiagnosticsAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((string _, ExtractorParameters _) => new List<DiagnosticTemplateResource>
                {
                    new DiagnosticTemplateResource
                    {
                        OriginalName = DefaultDiagnosticName,
                        Name = DefaultDiagnosticName,
                        Properties = new DiagnosticTemplateProperties()
                        {
                            LoggerId = LoggerIdValue
                        }
                    }
                });

            return mockDiagnosticClient.Object;
        }
    }
}
