// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    class MockPolicyClient
    {
        public const string TemplateName = "name";
        public const string GlobalPolicyContent = "<policies> my mocked policies </policies>";

        public static IPolicyClient GetMockedApiClientWithDefaultValues()
        {
            var mockPolicyApiClient = new Mock<IPolicyClient>(MockBehavior.Strict);

            mockPolicyApiClient
                .Setup(x => x.GetGlobalServicePolicyAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new PolicyTemplateResource
                {
                    Name = TemplateName,
                    Type = ResourceTypeConstants.GlobalServicePolicy,
                    Properties = new PolicyTemplateProperties
                    {
                        Format = "rawxml",
                        PolicyContent = GlobalPolicyContent
                    }
                });

            mockPolicyApiClient
                .Setup(x => x.GetPolicyLinkedToProductAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((string productName, ExtractorParameters _) => new PolicyTemplateResource
                {
                    Name = $"{productName}-{TemplateName}",
                    Type = ResourceTypeConstants.ProductPolicy,
                    Properties = new PolicyTemplateProperties
                    {
                        Format = "rawxml",
                        PolicyContent = GlobalPolicyContent
                    }
                });

            mockPolicyApiClient
                .Setup(x => x.GetPolicyLinkedToApiAsync(It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((string apiName, ExtractorParameters _) => new PolicyTemplateResource
                {
                    Name = $"{apiName}-{TemplateName}",
                    Type = ResourceTypeConstants.ProductPolicy,
                    Properties = new PolicyTemplateProperties
                    {
                        Format = "rawxml",
                        PolicyContent = GlobalPolicyContent
                    }
                });

            mockPolicyApiClient
                .Setup(x => x.GetPolicyLinkedToApiOperationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync((string apiName, string apiOperation, ExtractorParameters _) => new PolicyTemplateResource
                {
                    Name = $"{apiName}-{apiOperation}-{TemplateName}",
                    Type = ResourceTypeConstants.ProductPolicy,
                    Properties = new PolicyTemplateProperties
                    {
                        Format = "rawxml",
                        PolicyContent = GlobalPolicyContent
                    }
                });

            return mockPolicyApiClient.Object;
        }
    }
}
