// --------------------------------------------------------------------------
//  <copyright file="MockPolicyApiClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Moq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    class MockPolicyApiClient
    {
        public const string TemplateType = "Microsoft.ApiManagement/service/policies";
        public const string TemplateName = "name";

        public static PolicyTemplateProperties TemplateProperties = new PolicyTemplateProperties
        {
            Format = "rawxml",
            Value = @"<policies> my mocked policies </policies>"
        };

        public static IPolicyApiClient GetMockedApiClientWithDefaultValues()
        {
            var mockPolicyApiClient = new Mock<IPolicyApiClient>(MockBehavior.Strict);

            mockPolicyApiClient
                .Setup(x => x.GetGlobalServicePolicyAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new PolicyTemplateResource
                {
                    Name = TemplateName,
                    Type = TemplateType,
                    Properties = TemplateProperties
                });

            return mockPolicyApiClient.Object;
        }
    }
}
