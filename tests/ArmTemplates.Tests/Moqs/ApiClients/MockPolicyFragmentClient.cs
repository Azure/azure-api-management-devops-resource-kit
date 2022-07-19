// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.PolicyFragments;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    class MockPolicyFragmentClient
    {
        public static async Task<IPolicyFragmentsClient> GetMockedHttpPolicyFragmentClient(string responseFileLocation)
        {
            var mockedProcessor = new PolicyFragmentDataProcessor();
            var mockedClient = new Mock<PolicyFragmentsClient>(MockBehavior.Strict, await MockClientUtils.GenerateMockedIHttpClientFactoryWithResponse(responseFileLocation) , mockedProcessor);
            MockClientUtils.MockAuthOfApiClient(mockedClient);
            
            return mockedClient.Object;
        }
    }
}
