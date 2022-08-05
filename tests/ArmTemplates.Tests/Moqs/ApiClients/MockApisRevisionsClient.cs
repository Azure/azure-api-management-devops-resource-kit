// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiRevisions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    public class MockApisRevisionsClient
    {   
        public static IApiRevisionClient GetMockedApiRevisionClientWithDefaultValues()
        {
            var mockApiRevisionClient = new Mock<IApiRevisionClient>(MockBehavior.Loose);

            mockApiRevisionClient
                .Setup(x => x.GetApiRevisionsAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new List<ApiRevisionTemplateResource>
                {
                    new ApiRevisionTemplateResource(),
                    new ApiRevisionTemplateResource()
                });

            return mockApiRevisionClient.Object;
        }
    }
}
