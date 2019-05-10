using Xunit;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
{
    public class BackendTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateBackendTemplateFromCreatorConfig()
        {
            // arrange
            BackendTemplateCreator backendTemplateCreator = new BackendTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { backends = new List<BackendTemplateProperties>() };
            BackendTemplateProperties backend = new BackendTemplateProperties()
            {
                title = "title",
                description = "description",
                resourceId = "resourceId",
                url = "url",
                protocol = "protocol",
                proxy = new BackendProxy()
                {
                    url = "url",
                    username = "user",
                    password = "pass"
                },
                tls = new BackendTLS()
                {
                    validateCertificateChain = true,
                    validateCertificateName = true
                },
                credentials = new BackendCredentials()
                {
                    certificate = new string[] { "cert1" },
                    query = new object(),
                    header = new object(),
                    authorization = new BackendCredentialsAuthorization()
                    {
                        scheme = "scheme",
                        parameter = "parameter"
                    }
                },
                properties = new BackendSubProperties()
                {
                    serviceFabricCluster = new BackendServiceFabricCluster()
                    {
                        clientCertificatethumbprint = "",
                        managementEndpoints = new string[] { "endpoint" },
                        maxPartitionResolutionRetries = 1,
                        serverCertificateThumbprints = new string[] { "thumbprint" },
                        serverX509Names = new ServerX509Names[]{
                        new ServerX509Names(){
                            name = "name",
                            issuerCertificateThumbprint = "thumbprint"
                        } }
                    }
                }

            };
            creatorConfig.backends.Add(backend);

            // act
            Template backendTemplate = backendTemplateCreator.CreateBackendTemplate(creatorConfig);
            BackendTemplateResource backendTemplateResource = (BackendTemplateResource)backendTemplate.resources[0];

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{backend.title}')]", backendTemplateResource.name);
            Assert.Equal(backend.title, backendTemplateResource.properties.title);
            Assert.Equal(backend.description, backendTemplateResource.properties.description);
            Assert.Equal(backend.resourceId, backendTemplateResource.properties.resourceId);
            Assert.Equal(backend.url, backendTemplateResource.properties.url);
            Assert.Equal(backend.protocol, backendTemplateResource.properties.protocol);
            Assert.Equal(backend.proxy.url, backendTemplateResource.properties.proxy.url);
            Assert.Equal(backend.proxy.username, backendTemplateResource.properties.proxy.username);
            Assert.Equal(backend.proxy.password, backendTemplateResource.properties.proxy.password);
            Assert.Equal(backend.tls.validateCertificateChain, backendTemplateResource.properties.tls.validateCertificateChain);
            Assert.Equal(backend.tls.validateCertificateName, backendTemplateResource.properties.tls.validateCertificateName);
            Assert.Equal(backend.credentials.certificate, backendTemplateResource.properties.credentials.certificate);
            Assert.Equal(backend.credentials.query, backendTemplateResource.properties.credentials.query);
            Assert.Equal(backend.credentials.header, backendTemplateResource.properties.credentials.header);
            Assert.Equal(backend.credentials.authorization.scheme, backendTemplateResource.properties.credentials.authorization.scheme);
            Assert.Equal(backend.credentials.authorization.parameter, backendTemplateResource.properties.credentials.authorization.parameter);
            Assert.Equal(backend.properties.serviceFabricCluster.clientCertificatethumbprint, backendTemplateResource.properties.properties.serviceFabricCluster.clientCertificatethumbprint);
            Assert.Equal(backend.properties.serviceFabricCluster.managementEndpoints, backendTemplateResource.properties.properties.serviceFabricCluster.managementEndpoints);
            Assert.Equal(backend.properties.serviceFabricCluster.maxPartitionResolutionRetries, backendTemplateResource.properties.properties.serviceFabricCluster.maxPartitionResolutionRetries);
            Assert.Equal(backend.properties.serviceFabricCluster.serverCertificateThumbprints, backendTemplateResource.properties.properties.serviceFabricCluster.serverCertificateThumbprints);
            Assert.Equal(backend.properties.serviceFabricCluster.serverX509Names[0].issuerCertificateThumbprint, backendTemplateResource.properties.properties.serviceFabricCluster.serverX509Names[0].issuerCertificateThumbprint);
            Assert.Equal(backend.properties.serviceFabricCluster.serverX509Names[0].name, backendTemplateResource.properties.properties.serviceFabricCluster.serverX509Names[0].name);
        }
    }
}
