﻿// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class DiagnosticTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateDiagnosticTemplateResourceFromCreatorConfig()
        {
            // arrange
            DiagnosticTemplateCreator diagnosticTemplateCreator = new DiagnosticTemplateCreator();
            CreatorParameters creatorConfig = new CreatorParameters() { Apis = new List<ApiConfig>() };
            ApiConfig api = new ApiConfig()
            {
                Diagnostic = new DiagnosticConfig()
                {
                    Name = "applicationinsights",
                    AlwaysLog = "alwaysLog",
                    LoggerId = "loggerId",
                    Sampling = new DiagnosticTemplateSampling()
                    {
                        SamplingType = "samplingType",
                        Percentage = 100
                    },
                    Frontend = new DiagnosticTemplateFrontendBackend()
                    {
                        Request = new DiagnosticTemplateRequestResponse()
                        {
                            Headers = new string[] { "frontendrequestheader" },
                            Body = new DiagnosticTemplateRequestResponseBody()
                            {
                                Bytes = 512
                            }
                        },
                        Response = new DiagnosticTemplateRequestResponse()
                        {
                            Headers = new string[] { "frontendresponseheader" },
                            Body = new DiagnosticTemplateRequestResponseBody()
                            {
                                Bytes = 512
                            }
                        }
                    },
                    Backend = new DiagnosticTemplateFrontendBackend()
                    {
                        Request = new DiagnosticTemplateRequestResponse()
                        {
                            Headers = new string[] { "backendrequestheader" },
                            Body = new DiagnosticTemplateRequestResponseBody()
                            {
                                Bytes = 512
                            }
                        },
                        Response = new DiagnosticTemplateRequestResponse()
                        {
                            Headers = new string[] { "backendresponseheader" },
                            Body = new DiagnosticTemplateRequestResponseBody()
                            {
                                Bytes = 512
                            }
                        }
                    },
                    EnableHttpCorrelationHeaders = true
                }
            };
            creatorConfig.Apis.Add(api);

            // act
            string[] dependsOn = new string[] { "dependsOn" };
            DiagnosticTemplateResource diagnosticTemplateResource = diagnosticTemplateCreator.CreateAPIDiagnosticTemplateResource(api, dependsOn);

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.Name}/{api.Diagnostic.Name}')]", diagnosticTemplateResource.Name);
            Assert.Equal(dependsOn, diagnosticTemplateResource.DependsOn);
            Assert.Equal(api.Diagnostic.AlwaysLog, diagnosticTemplateResource.Properties.AlwaysLog);
            Assert.Equal($"[resourceId('Microsoft.ApiManagement/service/loggers', parameters('{ParameterNames.ApimServiceName}'), '{api.Diagnostic.LoggerId}')]", diagnosticTemplateResource.Properties.LoggerId);
            Assert.Equal(api.Diagnostic.EnableHttpCorrelationHeaders, diagnosticTemplateResource.Properties.EnableHttpCorrelationHeaders);
            Assert.Equal(api.Diagnostic.Sampling.SamplingType, diagnosticTemplateResource.Properties.Sampling.SamplingType);
            Assert.Equal(api.Diagnostic.Sampling.Percentage, diagnosticTemplateResource.Properties.Sampling.Percentage);
            Assert.Equal(api.Diagnostic.Frontend.Request.Headers, diagnosticTemplateResource.Properties.Frontend.Request.Headers);
            Assert.Equal(api.Diagnostic.Frontend.Request.Body.Bytes, diagnosticTemplateResource.Properties.Frontend.Request.Body.Bytes);
            Assert.Equal(api.Diagnostic.Frontend.Response.Headers, diagnosticTemplateResource.Properties.Frontend.Response.Headers);
            Assert.Equal(api.Diagnostic.Frontend.Response.Body.Bytes, diagnosticTemplateResource.Properties.Frontend.Response.Body.Bytes);
            Assert.Equal(api.Diagnostic.Backend.Request.Headers, diagnosticTemplateResource.Properties.Backend.Request.Headers);
            Assert.Equal(api.Diagnostic.Backend.Request.Body.Bytes, diagnosticTemplateResource.Properties.Backend.Request.Body.Bytes);
            Assert.Equal(api.Diagnostic.Backend.Response.Headers, diagnosticTemplateResource.Properties.Backend.Response.Headers);
            Assert.Equal(api.Diagnostic.Backend.Response.Body.Bytes, diagnosticTemplateResource.Properties.Backend.Response.Body.Bytes);
        }
    }
}
