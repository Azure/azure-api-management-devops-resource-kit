// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.ApiManagementService;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiManagementService;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;
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
                .Setup(x => x.GetApiManagementService(It.IsAny<ExtractorParameters>()))
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

        public static IApiManagementServiceClient GetMockedApiManagementServiceClientWithJsonResponse(string jsonResponse)
        {
            var mockedProcessor = new Mock<IApiManagementServiceProcessor>(MockBehavior.Loose).Object;

            var mockApiManagementServiceClient = new Mock<ApiManagementServiceClient>(MockBehavior.Strict, mockedProcessor) { CallBase = true };
            mockApiManagementServiceClient.Protected()
                .Setup<Task<string>>("CallApiManagementAsync", ItExpr.IsAny<string>(), ItExpr.IsAny<string>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<ClientHttpMethod>())
                .ReturnsAsync(jsonResponse);

            mockApiManagementServiceClient.Protected()
                .Setup<AzureCliAuthenticator>("Auth").Returns(GetMockedAzureClient());

            return mockApiManagementServiceClient.Object;
        }

        public static AzureCliAuthenticator GetMockedAzureClient()
        {
            var mockedZureClientAuth = new Mock<AzureCliAuthenticator>(MockBehavior.Strict);
            mockedZureClientAuth.Setup(x => x.GetAccessToken()).ReturnsAsync(() => ("val1", "val2"));
            return mockedZureClientAuth.Object;
        }
    }
}
