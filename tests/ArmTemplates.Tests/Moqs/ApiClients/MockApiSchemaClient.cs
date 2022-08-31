// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.ApiSchemas;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiSchemas;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    public class MockApiSchemaClient
    {
        public const string ApiSchemaName = "api-schema-name";
        public const string ApiSchemaDocComponents = "api-schema-document-components";

        public static IApiSchemaClient GetMockedApiClientWithDefaultValues()
        {
            var mockApiSchemaClient = new Mock<IApiSchemaClient>(MockBehavior.Strict);

            mockApiSchemaClient
                .Setup(x => x.GetApiSchemasAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new List<ApiSchemaTemplateResource>
                {
                    new ApiSchemaTemplateResource
                    {
                        Name = ApiSchemaName,
                        Properties = new ApiSchemaProperties
                        {
                            ContentType = "application/json",
                            Document = new ApiSchemaDocument
                            {
                                Components = ApiSchemaDocComponents,
                                Value = "value"
                            }
                        }
                    }
                });

            return mockApiSchemaClient.Object;
        }

        public static IApiSchemaClient GetMockedApiClientWithGraphQLSchemaValues()
        {
            // arrange
            FileReader fileReader = new FileReader();
            string fileLocation = Path.Combine("Resources", "Schemas", "schema.gql");

            Task<string> fileReadingTask = fileReader.RetrieveFileContentsAsync(fileLocation);

            var mockApiSchemaClient = new Mock<IApiSchemaClient>(MockBehavior.Strict);

            mockApiSchemaClient
                .Setup(x => x.GetApiSchemasAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new List<ApiSchemaTemplateResource>
                {
                    new ApiSchemaTemplateResource
                    {
                        Name = ApiSchemaName,
                        Properties = new ApiSchemaProperties
                        {
                            ContentType = "application/vnd.ms-azure-apim.graphql.schema",
                            Document = new ApiSchemaDocument
                            {
                                Components = ApiSchemaDocComponents,
                                Value = fileReadingTask.Result.ToString()
                            }
                        }
                    }
                });

            return mockApiSchemaClient.Object;
        }

        public static async Task<IApiSchemaClient> GetMockedHttpApiSchemaClient(string responseFileLocation)
        {
            var mockedClient= new Mock<ApiSchemaClient>(MockBehavior.Strict, await MockClientUtils.GenerateMockedIHttpClientFactoryWithResponse(responseFileLocation), new TemplateResourceDataProcessor<ApiSchemaTemplateResource>());
            MockClientUtils.MockAuthOfApiClient(mockedClient);

            return mockedClient.Object;
        }
    }
}
