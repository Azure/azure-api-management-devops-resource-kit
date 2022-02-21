using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class DiagnosticTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateDiagnosticTemplateResourceFromCreatorConfig()
        {
            // arrange
            DiagnosticTemplateCreator diagnosticTemplateCreator = new DiagnosticTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { apis = new List<APIConfig>() };
            APIConfig api = new APIConfig()
            {
                diagnostic = new DiagnosticConfig()
                {
                    name = "applicationinsights",
                    alwaysLog = "alwaysLog",
                    loggerId = "loggerId",
                    sampling = new DiagnosticTemplateSampling()
                    {
                        samplingType = "samplingType",
                        percentage = 100
                    },
                    frontend = new DiagnosticTemplateFrontendBackend()
                    {
                        request = new DiagnosticTemplateRequestResponse()
                        {
                            headers = new string[] { "frontendrequestheader" },
                            body = new DiagnosticTemplateRequestResponseBody()
                            {
                                bytes = 512
                            }
                        },
                        response = new DiagnosticTemplateRequestResponse()
                        {
                            headers = new string[] { "frontendresponseheader" },
                            body = new DiagnosticTemplateRequestResponseBody()
                            {
                                bytes = 512
                            }
                        }
                    },
                    backend = new DiagnosticTemplateFrontendBackend()
                    {
                        request = new DiagnosticTemplateRequestResponse()
                        {
                            headers = new string[] { "backendrequestheader" },
                            body = new DiagnosticTemplateRequestResponseBody()
                            {
                                bytes = 512
                            }
                        },
                        response = new DiagnosticTemplateRequestResponse()
                        {
                            headers = new string[] { "backendresponseheader" },
                            body = new DiagnosticTemplateRequestResponseBody()
                            {
                                bytes = 512
                            }
                        }
                    },
                    enableHttpCorrelationHeaders = true
                }
            };
            creatorConfig.apis.Add(api);

            // act
            string[] dependsOn = new string[] { "dependsOn" };
            DiagnosticTemplateResource diagnosticTemplateResource = diagnosticTemplateCreator.CreateAPIDiagnosticTemplateResource(api, dependsOn);

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{api.name}/{api.diagnostic.name}')]", diagnosticTemplateResource.Name);
            Assert.Equal(dependsOn, diagnosticTemplateResource.DependsOn);
            Assert.Equal(api.diagnostic.alwaysLog, diagnosticTemplateResource.Properties.alwaysLog);
            Assert.Equal($"[resourceId('Microsoft.ApiManagement/service/loggers', parameters('ApimServiceName'), '{api.diagnostic.loggerId}')]", diagnosticTemplateResource.Properties.loggerId);
            Assert.Equal(api.diagnostic.enableHttpCorrelationHeaders, diagnosticTemplateResource.Properties.enableHttpCorrelationHeaders);
            Assert.Equal(api.diagnostic.sampling.samplingType, diagnosticTemplateResource.Properties.sampling.samplingType);
            Assert.Equal(api.diagnostic.sampling.percentage, diagnosticTemplateResource.Properties.sampling.percentage);
            Assert.Equal(api.diagnostic.frontend.request.headers, diagnosticTemplateResource.Properties.frontend.request.headers);
            Assert.Equal(api.diagnostic.frontend.request.body.bytes, diagnosticTemplateResource.Properties.frontend.request.body.bytes);
            Assert.Equal(api.diagnostic.frontend.response.headers, diagnosticTemplateResource.Properties.frontend.response.headers);
            Assert.Equal(api.diagnostic.frontend.response.body.bytes, diagnosticTemplateResource.Properties.frontend.response.body.bytes);
            Assert.Equal(api.diagnostic.backend.request.headers, diagnosticTemplateResource.Properties.backend.request.headers);
            Assert.Equal(api.diagnostic.backend.request.body.bytes, diagnosticTemplateResource.Properties.backend.request.body.bytes);
            Assert.Equal(api.diagnostic.backend.response.headers, diagnosticTemplateResource.Properties.backend.response.headers);
            Assert.Equal(api.diagnostic.backend.response.body.bytes, diagnosticTemplateResource.Properties.backend.response.body.bytes);
        }
    }
}
