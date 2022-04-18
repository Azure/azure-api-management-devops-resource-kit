// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Xunit;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend;

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
                Title = "title",
                Description = "description",
                ResourceId = "resourceId",
                Url = "url",
                Protocol = "protocol",
                Proxy = new BackendProxy()
                {
                    Url = "url",
                    Username = "user",
                    Password = "pass"
                },
                Tls = new BackendTls()
                {
                    ValidateCertificateChain = true,
                    ValidateCertificateName = true
                },
                Credentials = new BackendCredentials()
                {
                    Certificate = new string[] { "cert1" },
                    Query = new object(),
                    Header = new object(),
                    Authorization = new BackendCredentialsAuthorization()
                    {
                        Scheme = "scheme",
                        Parameter = "parameter"
                    }
                },
                Properties = new BackendServiceFabricProperties()
                {
                    ServiceFabricCluster = new BackendServiceFabricCluster()
                    {
                        ClientCertificatethumbprint = "",
                        ManagementEndpoints = new string[] { "endpoint" },
                        MaxPartitionResolutionRetries = 1,
                        ServerCertificateThumbprints = new string[] { "thumbprint" },
                        ServerX509Names = new ServerX509Names[]{
                        new ServerX509Names(){
                            Name = "name",
                            IssuerCertificateThumbprint = "thumbprint"
                        } }
                    }
                }

            };
            creatorConfig.backends.Add(backend);

            // act
            Template backendTemplate = backendTemplateCreator.CreateBackendTemplate(creatorConfig);
            BackendTemplateResource backendTemplateResource = (BackendTemplateResource)backendTemplate.Resources[0];

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{backend.Title}')]", backendTemplateResource.Name);
            Assert.Equal(backend.Title, backendTemplateResource.Properties.Title);
            Assert.Equal(backend.Description, backendTemplateResource.Properties.Description);
            Assert.Equal(backend.ResourceId, backendTemplateResource.Properties.ResourceId);
            Assert.Equal(backend.Url, backendTemplateResource.Properties.Url);
            Assert.Equal(backend.Protocol, backendTemplateResource.Properties.Protocol);
            Assert.Equal(backend.Proxy.Url, backendTemplateResource.Properties.Proxy.Url);
            Assert.Equal(backend.Proxy.Username, backendTemplateResource.Properties.Proxy.Username);
            Assert.Equal(backend.Proxy.Password, backendTemplateResource.Properties.Proxy.Password);
            Assert.Equal(backend.Tls.ValidateCertificateChain, backendTemplateResource.Properties.Tls.ValidateCertificateChain);
            Assert.Equal(backend.Tls.ValidateCertificateName, backendTemplateResource.Properties.Tls.ValidateCertificateName);
            Assert.Equal(backend.Credentials.Certificate, backendTemplateResource.Properties.Credentials.Certificate);
            Assert.Equal(backend.Credentials.Query, backendTemplateResource.Properties.Credentials.Query);
            Assert.Equal(backend.Credentials.Header, backendTemplateResource.Properties.Credentials.Header);
            Assert.Equal(backend.Credentials.Authorization.Scheme, backendTemplateResource.Properties.Credentials.Authorization.Scheme);
            Assert.Equal(backend.Credentials.Authorization.Parameter, backendTemplateResource.Properties.Credentials.Authorization.Parameter);
            Assert.Equal(backend.Properties.ServiceFabricCluster.ClientCertificatethumbprint, backendTemplateResource.Properties.Properties.ServiceFabricCluster.ClientCertificatethumbprint);
            Assert.Equal(backend.Properties.ServiceFabricCluster.ManagementEndpoints, backendTemplateResource.Properties.Properties.ServiceFabricCluster.ManagementEndpoints);
            Assert.Equal(backend.Properties.ServiceFabricCluster.MaxPartitionResolutionRetries, backendTemplateResource.Properties.Properties.ServiceFabricCluster.MaxPartitionResolutionRetries);
            Assert.Equal(backend.Properties.ServiceFabricCluster.ServerCertificateThumbprints, backendTemplateResource.Properties.Properties.ServiceFabricCluster.ServerCertificateThumbprints);
            Assert.Equal(backend.Properties.ServiceFabricCluster.ServerX509Names[0].IssuerCertificateThumbprint, backendTemplateResource.Properties.Properties.ServiceFabricCluster.ServerX509Names[0].IssuerCertificateThumbprint);
            Assert.Equal(backend.Properties.ServiceFabricCluster.ServerX509Names[0].Name, backendTemplateResource.Properties.Properties.ServiceFabricCluster.ServerX509Names[0].Name);
        }
    }
}
