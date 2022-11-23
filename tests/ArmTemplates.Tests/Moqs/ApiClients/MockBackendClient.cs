// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Backend;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    class MockBackendClient
    {
        public const string BackendName = "backend-name";

        public static IBackendClient GetMockedApiClientWithDefaultValues()
        {
            var mockBackendClient = new Mock<IBackendClient>(MockBehavior.Strict);

            mockBackendClient
                .Setup(x => x.GetAllAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new List<BackendTemplateResource>
                {
                    new BackendTemplateResource
                    {
                        OriginalName = BackendName,
                        Name = BackendName,
                        Properties = new BackendTemplateProperties
                        {
                            Description = $"{BackendName}-description",
                            Url = $"{BackendName}-url",
                            Protocol = $"{BackendName}-protocol",
                            Proxy = new() 
                            {
                                Url = $"{BackendName}-proxy-url",
                                Username = $"{BackendName}-username",
                                Password = $"{BackendName}-secure-password"
                            }
                        }
                    }
                });

            return mockBackendClient.Object;
        }

        public static async Task<IBackendClient> GetMockedHttpApiClient(MockClientConfiguration mockClientConfiguration)
        {
            var mockedClient = new Mock<BackendClient>(MockBehavior.Strict, await MockClientUtils.GenerateMockedIHttpClientFactoryWithResponse(mockClientConfiguration), new TemplateResourceDataProcessor<BackendTemplateResource>());
            MockClientUtils.MockAuthOfApiClient(mockedClient);

            return mockedClient.Object;
        }
    }
}
