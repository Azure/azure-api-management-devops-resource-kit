// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiVersionSet;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    class MockApiVersionSetClient
    {
        public const string ApiVersionSetName1 = "api-version-set-1";
        public const string ApiVersionSetName2 = "api-version-set-2";

        public static IApiVersionSetClient GetMockedApiClientWithDefaultValues()
        {
            var mockApiVersionSetClient = new Mock<IApiVersionSetClient>(MockBehavior.Strict);

            mockApiVersionSetClient
                .Setup(x => x.GetAllAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new List<ApiVersionSetTemplateResource>
                {
                    new ApiVersionSetTemplateResource
                    {
                        Name = ApiVersionSetName1,
                        Type = ResourceTypeConstants.ApiVersionSet,
                        Properties = new ApiVersionSetProperties
                        {
                            DisplayName = $"{ApiVersionSetName1}-display-name",
                            Description = $"{ApiVersionSetName1}-description",
                            VersionHeaderName = $"{ApiVersionSetName1}-version-header",
                            VersioningScheme = $"Segment",
                            VersionQueryName = $"{ApiVersionSetName1}-version-query"
                        }
                    },

                    new ApiVersionSetTemplateResource
                    {
                        Name = ApiVersionSetName2,
                        Type = ResourceTypeConstants.ApiVersionSet,
                        Properties = new ApiVersionSetProperties
                        {
                            DisplayName = $"{ApiVersionSetName2}-display-name",
                            Description = $"{ApiVersionSetName2}-description",
                            VersionHeaderName = $"{ApiVersionSetName2}-version-header",
                            VersioningScheme = $"Query",
                            VersionQueryName = $"{ApiVersionSetName2}-version-query"
                        }
                    },
                });

            return mockApiVersionSetClient.Object;
        }
    }
}
