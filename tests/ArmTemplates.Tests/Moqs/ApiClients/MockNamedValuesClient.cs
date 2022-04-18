// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
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
                        Name = NamedValueName,
                        Properties = new()
                        {
                            DisplayName = NamedValueDisplayName
                        }
                    }
                });

            return mockNamedValuesClient.Object;
        }
    }
}
