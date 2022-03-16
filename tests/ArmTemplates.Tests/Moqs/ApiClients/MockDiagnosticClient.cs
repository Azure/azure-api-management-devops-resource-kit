﻿// --------------------------------------------------------------------------
//  <copyright file="MockDiagnosticClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
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

        public static IDiagnosticClient GetMockedApiClientWithDefaultValues()
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
                            loggerId = "logger-id"
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
                            loggerId = "logger-id"
                        }
                    }
                });

            return mockDiagnosticClient.Object;
        }
    }
}
