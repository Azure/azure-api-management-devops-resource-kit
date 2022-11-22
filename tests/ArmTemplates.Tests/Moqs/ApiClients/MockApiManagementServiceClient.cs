// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.ApiManagementService;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiManagementService;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors;
using Moq;
using Moq.Protected;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    public class MockApiManagementServiceClient
    {        
        public const string ServiceName = "api-management-service-name";

        public static IApiManagementServiceClient GetMockedApiManagementServiceClientWithDefaultValues()
        {
            var mockApiManagementServiceClient = new Mock<IApiManagementServiceClient>(MockBehavior.Strict);

            mockApiManagementServiceClient
                .Setup(x => x.GetApiManagementServiceAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((ExtractorParameters _) => new ApiManagementServiceResource
                {
                    OriginalName = ServiceName,
                    Name = ServiceName,
                    Tags = new Dictionary<string, string>() {
                        { "tag1key", "tag1val" },
                        { "tag2key", "tag2val" },
                    },
                    Location = "location-value"
                });

            return mockApiManagementServiceClient.Object;
        }

        public static async Task<IApiManagementServiceClient> GetMockedHttpApiManagementServiceClient(MockClientConfiguration mockClientConfiguration)
        {
            var dataProcessor = new TemplateResourceDataProcessor<ApiManagementServiceResource>();
            var mockedClient = new Mock<ApiManagementServiceClient>(MockBehavior.Strict, await MockClientUtils.GenerateMockedIHttpClientFactoryWithResponse(mockClientConfiguration), dataProcessor);
            mockedClient.Protected()
                .Setup<AzureCliAuthenticator>("Auth").Returns(MockClientUtils.GetMockedAzureClient());

            return mockedClient.Object;
        }
    }
}
