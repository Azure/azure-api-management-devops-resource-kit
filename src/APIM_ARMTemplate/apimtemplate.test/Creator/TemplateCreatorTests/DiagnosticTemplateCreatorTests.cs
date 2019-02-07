using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class DiagnosticTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateDiagnosticTemplateResourceFromCreatorConfig()
        {
            // arrange
            DiagnosticTemplateCreator diagnosticTemplateCreator = new DiagnosticTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig()
            {
                api = new APIConfig()
                {
                    name = "name"
                },
                diagnostic = new DiagnosticTemplateProperties()
                {
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

            // act
            string[] dependsOn = new string[] { "dependsOn" };
            DiagnosticTemplateResource diagnosticTemplateResource = diagnosticTemplateCreator.CreateAPIDiagnosticTemplateResource(creatorConfig, dependsOn);

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{creatorConfig.api.name}/diagnostic')]", diagnosticTemplateResource.name);
            Assert.Equal(dependsOn, diagnosticTemplateResource.dependsOn);
            Assert.Equal(creatorConfig.diagnostic.alwaysLog, diagnosticTemplateResource.properties.alwaysLog);
            Assert.Equal(creatorConfig.diagnostic.loggerId, diagnosticTemplateResource.properties.loggerId);
            Assert.Equal(creatorConfig.diagnostic.enableHttpCorrelationHeaders, diagnosticTemplateResource.properties.enableHttpCorrelationHeaders);
            Assert.Equal(creatorConfig.diagnostic.sampling.samplingType, diagnosticTemplateResource.properties.sampling.samplingType);
            Assert.Equal(creatorConfig.diagnostic.sampling.percentage, diagnosticTemplateResource.properties.sampling.percentage);
            Assert.Equal(creatorConfig.diagnostic.frontend.request.headers, diagnosticTemplateResource.properties.frontend.request.headers);
            Assert.Equal(creatorConfig.diagnostic.frontend.request.body.bytes, diagnosticTemplateResource.properties.frontend.request.body.bytes);
            Assert.Equal(creatorConfig.diagnostic.frontend.response.headers, diagnosticTemplateResource.properties.frontend.response.headers);
            Assert.Equal(creatorConfig.diagnostic.frontend.response.body.bytes, diagnosticTemplateResource.properties.frontend.response.body.bytes);
            Assert.Equal(creatorConfig.diagnostic.backend.request.headers, diagnosticTemplateResource.properties.backend.request.headers);
            Assert.Equal(creatorConfig.diagnostic.backend.request.body.bytes, diagnosticTemplateResource.properties.backend.request.body.bytes);
            Assert.Equal(creatorConfig.diagnostic.backend.response.headers, diagnosticTemplateResource.properties.backend.response.headers);
            Assert.Equal(creatorConfig.diagnostic.backend.response.body.bytes, diagnosticTemplateResource.properties.backend.response.body.bytes);
        }
    }
}
