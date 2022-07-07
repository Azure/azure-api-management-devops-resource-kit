// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Schemas;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors;
using Moq;
using Moq.Protected;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    class MockSchemaClient
    {
        public static async Task<ISchemasClient> GetMockedHttpSchemaClient(string responseFileLocation)
        {
            var dataProcessor = new SchemaDataProcessor();
            var mockedClient = new Mock<SchemasClient>(MockBehavior.Strict, await MockClientUtils.GenerateMockedIHttpClientFactoryWithResponse(responseFileLocation) , dataProcessor);
            mockedClient.Protected()
                .Setup<AzureCliAuthenticator>("Auth").Returns(MockClientUtils.GetMockedAzureClient());

            return mockedClient.Object;
        }
    }
}
