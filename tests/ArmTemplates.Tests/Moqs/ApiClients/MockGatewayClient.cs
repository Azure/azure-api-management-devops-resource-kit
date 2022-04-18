// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Gateway;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    class MockGatewayClient
    {
        public const string GatewayName1 = "gateway-name-1";
        public const string GatewayName2 = "gateway-name-2";

        public static IGatewayClient GetMockedApiClientWithDefaultValues()
        {
            var mockGatewayClient = new Mock<IGatewayClient>(MockBehavior.Strict);

            mockGatewayClient
                .Setup(x => x.GetAllAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((ExtractorParameters _) => new List<GatewayTemplateResource>
                {
                    new GatewayTemplateResource
                    {
                        Name = GatewayName1,
                        Type = ResourceTypeConstants.Gateway,
                        Properties = new GatewayProperties
                        {
                            Description = $"description-{GatewayName1}",
                            LocationData = new()
                            {
                                Name = $"location-data-{GatewayName1}",
                                City = $"city-{GatewayName1}",
                                District = $"district-{GatewayName1}",
                                CountryOrRegion = $"country-or-region-{GatewayName1}"
                            }
                        }
                    },

                    new GatewayTemplateResource
                    {
                        Name = GatewayName2,
                        Type = ResourceTypeConstants.Gateway,
                        Properties = new GatewayProperties
                        {
                            Description = $"description-{GatewayName2}",
                            LocationData = new()
                            {
                                Name = $"location-data-{GatewayName2}",
                                City = $"city-{GatewayName2}",
                                District = $"district-{GatewayName2}",
                                CountryOrRegion = $"country-or-region-{GatewayName2}"
                            }
                        }
                    }
                });

            return mockGatewayClient.Object;
        }
    }
}
