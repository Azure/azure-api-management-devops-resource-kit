// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiSchemas;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
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
    }
}
