// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.NamedValues;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    public class MockNamedValuesClient
    {
        public const string NamedValueName = "named-value---name";
        public const string NamedValueDisplayName = "named-value---display-name";

        public static INamedValuesClient GetMockedApiClientWithDefaultValues()
        {
            var mockNamedValuesClient = new Mock<INamedValuesClient>(MockBehavior.Strict);

            mockNamedValuesClient
                .Setup(x => x.GetAllAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((ExtractorParameters _) => new List<NamedValueTemplateResource>()
                {
                    new NamedValueTemplateResource
                    {
                        OriginalName = NamedValueName,
                        Name = NamedValueName,
                        Properties = new()
                        {
                            DisplayName = NamedValueDisplayName
                        }
                    }
                });
            return mockNamedValuesClient.Object;
        }

        public static async Task<INamedValuesClient> GetMockedHttpNamedValuesClient(params MockClientConfiguration[] mockClientConfigurations)
        {
            var dataProcessor = new NamedValuesDataProcessor();
            var mockedClient = new Mock<NamedValuesClient>(MockBehavior.Strict, await MockClientUtils.GenerateMockedIHttpClientFactoryWithResponse(mockClientConfigurations), dataProcessor);
            MockClientUtils.MockAuthOfApiClient(mockedClient);

            return mockedClient.Object;
        }
    }
}
