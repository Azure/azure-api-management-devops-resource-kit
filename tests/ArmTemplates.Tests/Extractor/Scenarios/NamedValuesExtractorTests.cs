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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger;
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
    [Trait("Category", "Named Values Extraction")]
    public class NamedValuesExtractorTests : ExtractorMockerWithOutputTestsBase
    {        
        public NamedValuesExtractorTests() : base("named-values-tests")
        {
        }

        [Fact]
        public async Task GenerateNamedValuesTemplates_ProperlyLaysTheInformation()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateNamedValuesTemplates_ProperlyLaysTheInformation));

            var extractorConfig = this.GetMockedExtractorConsoleAppConfiguration(
                splitApis: false,
                apiVersionSetName: string.Empty,
                multipleApiNames: string.Empty,
                includeAllRevisions: false,
                toNotIncludeNamedValue: false);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var mockPolicyExtractor = new Mock<IPolicyExtractor>(MockBehavior.Strict);
            mockPolicyExtractor
                .Setup(x => x.GetCachedPolicyContent(It.IsAny<PolicyTemplateResource>(), It.IsAny<string>()))
                .Returns((PolicyTemplateResource policy, string _) => $"mock-response-from-policy-{policy.Name}");

            var mockBackendExtractor = new Mock<IBackendExtractor>(MockBehavior.Strict);
            mockBackendExtractor
                .Setup(x => x.IsNamedValueUsedInBackends(
                    It.IsAny<string>(), 
                    It.IsAny<List<PolicyTemplateResource>>(), 
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ExtractorParameters>(),
                    It.IsAny<string>()))
                .ReturnsAsync(false);

            var mockedNamedValuesClient = MockNamedValuesClient.GetMockedApiClientWithDefaultValues();
            var namedValuesExtractor = new NamedValuesExtractor(
                this.GetTestLogger<NamedValuesExtractor>(), 
                new TemplateBuilder(), 
                mockedNamedValuesClient,
                mockPolicyExtractor.Object,
                mockBackendExtractor.Object);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                namedValuesExtractor: namedValuesExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var namedValuesTemplate = await extractorExecutor.GenerateNamedValuesTemplateAsync(
                "some-single-api",
                new List<PolicyTemplateResource>() 
                { 
                    new PolicyTemplateResource { Name = MockNamedValuesClient.NamedValueName },
                    new PolicyTemplateResource { Name = MockNamedValuesClient.NamedValueDisplayName }
                },
                It.IsAny<List<LoggerTemplateResource>>(),
                currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.NamedValues)).Should().BeTrue();

            namedValuesTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            namedValuesTemplate.Parameters.Should().ContainKey(ParameterNames.NamedValues);
            namedValuesTemplate.Parameters.Should().ContainKey(ParameterNames.NamedValueKeyVaultSecrets);
            
            namedValuesTemplate.TypedResources.NamedValues.Should().HaveCount(1);
            namedValuesTemplate.Resources.Should().HaveCount(1);

            namedValuesTemplate.TypedResources.NamedValues.First().Type.Should().Be(ResourceTypeConstants.NamedValues);
            namedValuesTemplate.TypedResources.NamedValues.First().Name.Should().Contain(MockNamedValuesClient.NamedValueName);
            namedValuesTemplate.TypedResources.NamedValues.First().Properties.DisplayName.Should().Contain(MockNamedValuesClient.NamedValueDisplayName);
        }

        [Fact]
        public async Task GenerateNamedValuesTemplates_ProperlyLaysTheInformation_GivenExtractSecretSetToTrue()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateNamedValuesTemplates_ProperlyLaysTheInformation_GivenExtractSecretSetToTrue));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(extractSecrets: "true");
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var fileLocationNamedValues = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListNamedValues_success_response.json");
            var fileLocationSecretValues = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementNamedValueListValue_success_response.json");
            var mockedNamedValuesClient = await MockNamedValuesClient.GetMockedHttpNamedValuesClient(
                new MockClientConfiguration(responseFileLocation: fileLocationNamedValues, urlPath: $"/namedValues?api-version={GlobalConstants.ApiVersion}"),
                new MockClientConfiguration(responseFileLocation: fileLocationSecretValues, urlPath: $"/namedValues/named-value-2/listValue?api-version={GlobalConstants.ApiVersion}"));

            var namedValuesExtractor = new NamedValuesExtractor(
                this.GetTestLogger<NamedValuesExtractor>(),
                new TemplateBuilder(),
                mockedNamedValuesClient,
                default,
                default);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                namedValuesExtractor: namedValuesExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var namedValuesTemplate = await extractorExecutor.GenerateNamedValuesTemplateAsync(
                null,
                new List<PolicyTemplateResource>(){},
                It.IsAny<List<LoggerTemplateResource>>(),
                currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.NamedValues)).Should().BeTrue();

            namedValuesTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);

            namedValuesTemplate.TypedResources.NamedValues.Should().HaveCount(2);
            namedValuesTemplate.Resources.Should().HaveCount(2);

            var plainNamedValue = namedValuesTemplate.TypedResources.NamedValues.First(x => x.OriginalName.Equals("named-value-1"));
            plainNamedValue.Should().NotBe(null);
            plainNamedValue.Properties.Value.Should().Be("named-value-1-value");

            var secretNamedValue = namedValuesTemplate.TypedResources.NamedValues.First(x => x.OriginalName.Equals("named-value-2"));
            secretNamedValue.Should().NotBe(null);
            secretNamedValue.Properties.Value.Should().Be("named-value-2");
        }
    }
}
