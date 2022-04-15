// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
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
                        Name = BackendName,
                        Properties = new BackendTemplateProperties
                        {
                            Description = $"{BackendName}-description",
                            Credentials = new(),
                            Tls = new(),
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
    }
}
