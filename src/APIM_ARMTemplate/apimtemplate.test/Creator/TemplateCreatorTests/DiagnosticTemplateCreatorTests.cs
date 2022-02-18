using Xunit;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
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
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{api.name}/{api.diagnostic.name}')]", diagnosticTemplateResource.name);
            Assert.Equal(dependsOn, diagnosticTemplateResource.dependsOn);
            Assert.Equal(api.diagnostic.alwaysLog, diagnosticTemplateResource.properties.alwaysLog);
            Assert.Equal($"[resourceId('Microsoft.ApiManagement/service/loggers', parameters('ApimServiceName'), '{api.diagnostic.loggerId}')]", diagnosticTemplateResource.properties.loggerId);
            Assert.Equal(api.diagnostic.enableHttpCorrelationHeaders, diagnosticTemplateResource.properties.enableHttpCorrelationHeaders);
            Assert.Equal(api.diagnostic.sampling.samplingType, diagnosticTemplateResource.properties.sampling.samplingType);
            Assert.Equal(api.diagnostic.sampling.percentage, diagnosticTemplateResource.properties.sampling.percentage);
            Assert.Equal(api.diagnostic.frontend.request.headers, diagnosticTemplateResource.properties.frontend.request.headers);
            Assert.Equal(api.diagnostic.frontend.request.body.bytes, diagnosticTemplateResource.properties.frontend.request.body.bytes);
            Assert.Equal(api.diagnostic.frontend.response.headers, diagnosticTemplateResource.properties.frontend.response.headers);
            Assert.Equal(api.diagnostic.frontend.response.body.bytes, diagnosticTemplateResource.properties.frontend.response.body.bytes);
            Assert.Equal(api.diagnostic.backend.request.headers, diagnosticTemplateResource.properties.backend.request.headers);
            Assert.Equal(api.diagnostic.backend.request.body.bytes, diagnosticTemplateResource.properties.backend.request.body.bytes);
            Assert.Equal(api.diagnostic.backend.response.headers, diagnosticTemplateResource.properties.backend.response.headers);
            Assert.Equal(api.diagnostic.backend.response.body.bytes, diagnosticTemplateResource.properties.backend.response.body.bytes);
        }
    }
}
