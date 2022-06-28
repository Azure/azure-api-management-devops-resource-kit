// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiManagementService;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Moq;

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
    }
}
