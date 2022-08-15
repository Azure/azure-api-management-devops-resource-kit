// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Api operations Extraction")]
    public class ApiOperationExtractorTests : ExtractorMockerWithOutputTestsBase
    {        
        public ApiOperationExtractorTests() : base("api-operations-tests")
        {
        }

        [Fact]
        public async Task GenerateApiOperationsResourcesAsync_ProperlyParsesResponse()
        {
            // arrange
            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                apiName: "apiName");
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var fileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListApiOperations_success_response.json");
            var mockedClient = await MockApiOperationClient.GetMockedHttpApiOperationClient(fileLocation);
            var apiOperationExtractor = new ApiOperationExtractor(this.GetTestLogger<ApiOperationExtractor>(), mockedClient);

            // act
            var apiOperations = await apiOperationExtractor.GenerateApiOperationsResourcesAsync("apiName", extractorParameters);

            // assert
            apiOperations.Count.Should().Be(5);

            var submitOrderOperation = apiOperations.First(x => x.Properties.DisplayName.Equals("submitOrder"));

            var exampleDefault = submitOrderOperation.Properties.Request.Representations[0].Examples["default"];
            exampleDefault.Value.Should().Be("[[{\"key\":\"value\"}]");
        }
    }
}
