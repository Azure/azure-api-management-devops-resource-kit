using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class BackendTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateBackendTemplateFromCreatorConfig()
        {
            // arrange
            BackendTemplateCreator backendTemplateCreator = new BackendTemplateCreator(new TemplateBuilder());
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
            BackendTemplateResource backendTemplateResource = (BackendTemplateResource)backendTemplate.Resources[0];

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{backend.title}')]", backendTemplateResource.Name);
            Assert.Equal(backend.title, backendTemplateResource.Properties.title);
            Assert.Equal(backend.description, backendTemplateResource.Properties.description);
            Assert.Equal(backend.resourceId, backendTemplateResource.Properties.resourceId);
            Assert.Equal(backend.url, backendTemplateResource.Properties.url);
            Assert.Equal(backend.protocol, backendTemplateResource.Properties.protocol);
            Assert.Equal(backend.proxy.url, backendTemplateResource.Properties.proxy.url);
            Assert.Equal(backend.proxy.username, backendTemplateResource.Properties.proxy.username);
            Assert.Equal(backend.proxy.password, backendTemplateResource.Properties.proxy.password);
            Assert.Equal(backend.tls.validateCertificateChain, backendTemplateResource.Properties.tls.validateCertificateChain);
            Assert.Equal(backend.tls.validateCertificateName, backendTemplateResource.Properties.tls.validateCertificateName);
            Assert.Equal(backend.credentials.certificate, backendTemplateResource.Properties.credentials.certificate);
            Assert.Equal(backend.credentials.query, backendTemplateResource.Properties.credentials.query);
            Assert.Equal(backend.credentials.header, backendTemplateResource.Properties.credentials.header);
            Assert.Equal(backend.credentials.authorization.scheme, backendTemplateResource.Properties.credentials.authorization.scheme);
            Assert.Equal(backend.credentials.authorization.parameter, backendTemplateResource.Properties.credentials.authorization.parameter);
            Assert.Equal(backend.properties.serviceFabricCluster.clientCertificatethumbprint, backendTemplateResource.Properties.properties.serviceFabricCluster.clientCertificatethumbprint);
            Assert.Equal(backend.properties.serviceFabricCluster.managementEndpoints, backendTemplateResource.Properties.properties.serviceFabricCluster.managementEndpoints);
            Assert.Equal(backend.properties.serviceFabricCluster.maxPartitionResolutionRetries, backendTemplateResource.Properties.properties.serviceFabricCluster.maxPartitionResolutionRetries);
            Assert.Equal(backend.properties.serviceFabricCluster.serverCertificateThumbprints, backendTemplateResource.Properties.properties.serviceFabricCluster.serverCertificateThumbprints);
            Assert.Equal(backend.properties.serviceFabricCluster.serverX509Names[0].issuerCertificateThumbprint, backendTemplateResource.Properties.properties.serviceFabricCluster.serverX509Names[0].issuerCertificateThumbprint);
            Assert.Equal(backend.properties.serviceFabricCluster.serverX509Names[0].name, backendTemplateResource.Properties.properties.serviceFabricCluster.serverX509Names[0].name);
        }
    }
}
