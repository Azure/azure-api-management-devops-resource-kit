// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    public class MockDiagnosticClient
    {
        public const string DiagnosticName = "diagnostic";
        public const string DefaultDiagnosticName = "default-diagnostic";

        public static IDiagnosticClient GetMockedClientWithApiDependentValues()
        {
            var mockDiagnosticClient = new Mock<IDiagnosticClient>(MockBehavior.Strict);

            mockDiagnosticClient
                .Setup(x => x.GetAllAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((ExtractorParameters _) => new List<DiagnosticTemplateResource>
                {
                    new DiagnosticTemplateResource
                    {
                        Name = DiagnosticName,
                        Properties = new DiagnosticTemplateProperties()
                        {
                            LoggerId = "logger-id"
                        }
                    }
                });

            mockDiagnosticClient
                .Setup(x => x.GetApiDiagnosticsAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((string apiName, ExtractorParameters _) => new List<DiagnosticTemplateResource>
                {
                    new DiagnosticTemplateResource
                    {
                        Name = $"{apiName}-{DiagnosticName}",
                        Properties = new DiagnosticTemplateProperties()
                        {
                            LoggerId = "logger-id"
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
                        Name = DefaultDiagnosticName,
                        Properties = new DiagnosticTemplateProperties()
                        {
                            LoggerId = "logger-id"
                        }
                    }
                });

            mockDiagnosticClient
                .Setup(x => x.GetApiDiagnosticsAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((string _, ExtractorParameters _) => new List<DiagnosticTemplateResource>
                {
                    new DiagnosticTemplateResource
                    {
                        Name = DefaultDiagnosticName,
                        Properties = new DiagnosticTemplateProperties()
                        {
                            LoggerId = "logger-id"
                        }
                    }
                });

            return mockDiagnosticClient.Object;
        }
    }
}
