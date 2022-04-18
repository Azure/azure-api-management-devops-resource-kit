// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients;
using Moq;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Backend Extraction")]
    public class BackendExtractorTests : ExtractorMockerWithOutputTestsBase
    {
        public BackendExtractorTests() : base("backend-tests")
        {
        }

        [Fact]
        public async Task GenerateBackendTemplates_ProperlyLaysTheInformation()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateBackendTemplates_ProperlyLaysTheInformation));

            var extractorConfig = this.GetMockedExtractorConsoleAppConfiguration(
                splitApis: false,
                apiVersionSetName: string.Empty,
                multipleApiNames: string.Empty,
                includeAllRevisions: false);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var mockPolicyExtractor = new Mock<IPolicyExtractor>(MockBehavior.Strict);
            mockPolicyExtractor
                .Setup(x => x.GetCachedPolicyContent(It.IsAny<PolicyTemplateResource>(), It.IsAny<string>()))
                .Returns((PolicyTemplateResource policy, string _) => $"mock-response-from-policy-{policy.Name}");

            var mockBackendClient = MockBackendClient.GetMockedApiClientWithDefaultValues();
            var backendExtractor = new BackendExtractor(
                this.GetTestLogger<BackendExtractor>(), 
                new TemplateBuilder(), 
                mockPolicyExtractor.Object,
                mockBackendClient);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                backendExtractor: backendExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var backendTemplate = await extractorExecutor.GenerateBackendTemplateAsync(
                singleApiName: null,
                new List<PolicyTemplateResource>(),
                new List<NamedValueTemplateResource>(),
                currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Backends)).Should().BeTrue();

            backendTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            backendTemplate.Parameters.Should().ContainKey(ParameterNames.BackendSettings);

            backendTemplate.TypedResources.Backends.Should().HaveCount(1);
            backendTemplate.Resources.Should().HaveCount(1);

            backendTemplate.TypedResources.Backends.First().Type.Should().Be(ResourceTypeConstants.Backend);
            backendTemplate.TypedResources.Backends.First().Name.Should().Contain(MockBackendClient.BackendName);

            var backendProperties = backendTemplate.TypedResources.Backends.First().Properties;
            backendProperties.Should().NotBeNull();
            backendProperties.Url.Should().Contain(ParameterNames.BackendSettings);
            backendProperties.Protocol.Should().Contain(ParameterNames.BackendSettings);
        }
    }
}
