// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.ApiOperations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiOperations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    public class MockApiOperationClient
    {
        public const string DisplayName = "api-operation-display";
        public const string Method = "api-operation-method";
        public const string UrlTemplate = "api-operation-url";
        public const string Description = "api-operation-description";

        public static ApiOperationRequest MockApiOperationRequest = new()
        {
            Description = "api-operation-request-description",
            Headers = new [] 
            { 
                new ApiOperationHeader
                {
                    Name = "header-name",
                    Description = "header-description-name",
                    DefaultValue = "default-value"
                }
            },
            QueryParameters = new []
            {
                new ApiOperationQueryParameter
                {
                    Name = "api-operation-query-name",
                    Required = true,
                    SchemaId = "api-operation-query-schema-id"
                }
            },
            Representations = new []
            {
                new ApiOperationRepresentation
                {
                    ContentType = "application/json",
                    SchemaId = "api-operation-representation-schema-id",
                    TypeName = "api-operation-representation-type-name",
                    Examples = new Dictionary<string, ParameterExampleContract>() {
                        {"default", new ParameterExampleContract { Description = "description", Value = new object(), Summary = "summary" } }
                    }
                }
            }
        };

        public static ApiOperationResponse MockApiOperationResponse = new()
        {
            Description = "api-operation-request-description",
            StatusCode = 200,
            Headers = new[]
            {
                new ApiOperationHeader
                {
                    Name = "header-name",
                    Description = "header-description-name",
                    DefaultValue = "default-value"
                }
            },
            Representations = new[]
            {
                new ApiOperationRepresentation
                {
                    ContentType = "application/json",
                    SchemaId = "api-operation-representation-schema-id",
                    TypeName = "api-operation-representation-type-name",
                    Examples = new Dictionary<string, ParameterExampleContract>() {
                        {"default", new ParameterExampleContract { Description = "description", Value = "plain value", Summary = "summary" } },
                        {"not default", new ParameterExampleContract { Description = "description", Value = new object(), Summary = "summary" } }
                    }
                }
            }
        };

        public static IApiOperationClient GetMockedApiClientWithDefaultValues()
        {
            var mockApiOperationClient = new Mock<IApiOperationClient>(MockBehavior.Strict);

            mockApiOperationClient
                .Setup(x => x.GetOperationsLinkedToApiAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((string apiName, ExtractorParameters _) => new List<ApiOperationTemplateResource>
                {
                    new ApiOperationTemplateResource
                    {
                        Name = "api-operation-name",
                        Type = ResourceTypeConstants.APIOperation,
                        Properties = new ApiOperationProperties
                        {
                            Request = MockApiOperationRequest,
                            Responses = new[] { MockApiOperationResponse }
                        }
                    }
                });

            return mockApiOperationClient.Object;
        }

        public static async Task<IApiOperationClient> GetMockedHttpApiOperationClient(MockClientConfiguration mockClientConfiguration)
        {
            var dataProcessor = new ApiOperationDataProcessor();
            var mockedClient = new Mock<ApiOperationClient>(MockBehavior.Strict, await MockClientUtils.GenerateMockedIHttpClientFactoryWithResponse(mockClientConfiguration), dataProcessor);
            MockClientUtils.MockAuthOfApiClient(mockedClient);

            return mockedClient.Object;
        }
    }
}
