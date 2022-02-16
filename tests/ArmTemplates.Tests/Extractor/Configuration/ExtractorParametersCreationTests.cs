// --------------------------------------------------------------------------
//  <copyright file="ExtractorParametersCreationTests.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using FluentAssertions;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Configuration
{
    [Trait("Category", "Unit")]
    public class ExtractorParametersCreationTests : ExtractorTestsBase
    {
        [Fact]
        public void ExtractorConfigValidate_NoPropertiesSet_MissingParameterException()
        {
            // arrange
            var defaultExtractorConfig = this.GetDefaultExtractorConfiguration();

            // act
            var extractorParameters = new ExtractorParameters(defaultExtractorConfig);

            // assert all config arguments were included to parameters
            // simple assertions
            extractorParameters.SourceApimName.Should().Be(MockSourceApimName);
            extractorParameters.DestinationApimName.Should().Be(MockDestinationApimName);
            extractorParameters.ResourceGroup.Should().Be(MockResourceGroup);
            extractorParameters.FilesGenerationRootDirectory.Should().Be(MockFileFolder);
            extractorParameters.SingleApiName.Should().Be(MockApiName);
            extractorParameters.SplitApis.Should().Be(MockSplitApis);
            extractorParameters.IncludeAllRevisions.Should().Be(MockIncludeAllRevisions);
            extractorParameters.LinkedTemplatesBaseUrl.Should().Be(MockLinkedTemplatesBaseUrl);
            extractorParameters.LinkedTemplatesSasToken.Should().Be(MockLinkedTemplatesSasToken);
            extractorParameters.LinkedTemplatesUrlQueryString.Should().Be(MockLinkedTemplatesUrlQueryString);
            extractorParameters.PolicyXMLBaseUrl.Should().Be(MockPolicyXMLBaseUrl);
            extractorParameters.PolicyXMLSasToken.Should().Be(MockPolicyXMLSasToken);
            extractorParameters.ApiVersionSetName.Should().Be(MockApiVersionSetName);
            extractorParameters.ServiceUrlParameters.Should().Contain(MockServiceUrlParameters);
            extractorParameters.ToParameterizeServiceUrl.Should().Be(MockToParameterizeServiceUrl);
            extractorParameters.ToParameterizeNamedValue.Should().Be(MockToParameterizeNamedValue);
            extractorParameters.ToParameterizeApiLoggerId.Should().Be(MockToParameterizeApiLoggerId);
            extractorParameters.ToParameterizeLogResourceId.Should().Be(MockToParameterizeLogResourceId);
            extractorParameters.NotIncludeNamedValue.Should().Be(MockNotIncludeNamedValue);
            extractorParameters.ParamNamedValuesKeyVaultSecrets.Should().Be(MockToParameterizeNamedValuesKeyVaultSecrets);
            extractorParameters.OperationBatchSize.Should().Be(MockOperationBatchSize);
            extractorParameters.ToParameterizeBackend.Should().Be(MockToParameterizeBackend);

            // more complicated assertions with parsing arguments
            extractorParameters.MultipleApiNames.Should().Contain(new[] { "test-multiple-api-1", "test-multiple-api-2" });

            extractorParameters.FileNames.Should().NotBeNull();
        }

        [Fact]
        public void ExtractorConfigValidate_MultipleApisNoStringPassed_DoesntThrowAnyException()
        {
            // arrange
            var defaultExtractorConfig = this.GetDefaultExtractorConfiguration();
            defaultExtractorConfig.MultipleAPIs = string.Empty;

            // act
            var createExtractorParameters = () => { _ = new ExtractorParameters(defaultExtractorConfig); };
            createExtractorParameters.Should().NotThrow();
        }
    }
}
