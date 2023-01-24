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
            var mockedClient = await MockApiOperationClient.GetMockedHttpApiOperationClient(new MockClientConfiguration(responseFileLocation: fileLocation));
            var apiOperationExtractor = new ApiOperationExtractor(this.GetTestLogger<ApiOperationExtractor>(), mockedClient);

            // act
            var apiOperations = await apiOperationExtractor.GenerateApiOperationsResourcesAsync("apiName", extractorParameters);

            // assert
            apiOperations.Count.Should().Be(5);

            var submitOrderOperation = apiOperations.First(x => x.Properties.DisplayName.Equals("submitOrder"));

            var exampleDefault = submitOrderOperation.Properties.Request.Representations[0].Examples["default"];
            exampleDefault.Value.Should().Be("[[{\"key\":\"value\"}]");

            var preferHeader = submitOrderOperation.Properties.Request.Headers.First(h => h.Name == "Prefer");
            preferHeader.Values.Should().Equal(new[] { "return=minimal", "return=representation" });
            preferHeader.DefaultValue.Should().Be("return=minimal");
            preferHeader.Type.Should().Be("string");
            preferHeader.Required.Should().BeFalse();

            var authorizationHeader = submitOrderOperation.Properties.Request.Headers.First(h => h.Name == "Authorization");
            authorizationHeader.Values.Should().BeEmpty();
            authorizationHeader.DefaultValue.Should().BeNull();
            authorizationHeader.Type.Should().Be("string");
            authorizationHeader.Required.Should().BeTrue();

            var maxCountHeader = submitOrderOperation.Properties.Request.Headers.First(h => h.Name == "MaxCount");
            maxCountHeader.Values.Should().BeEmpty();
            maxCountHeader.DefaultValue.Should().Be("100");
            maxCountHeader.Type.Should().Be("integer");
            maxCountHeader.Required.Should().BeFalse();
        }
    }
}
