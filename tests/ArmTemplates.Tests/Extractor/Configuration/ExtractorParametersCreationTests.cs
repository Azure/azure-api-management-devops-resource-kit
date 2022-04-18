// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Configuration
{
    [Trait("Category", "Unit")]
    public class ExtractorParametersCreationTests : ExtractorMockerTestsBase
    {
        [Fact]
        public void ExtractorConfigValidate_NoPropertiesSet_MissingParameterException()
        {
            // arrange
            var defaultExtractorConfig = this.GetMockedExtractorConsoleAppConfiguration();

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
            extractorParameters.ParameterizeServiceUrl.Should().Be(MockParameterizeServiceUrl);
            extractorParameters.ParameterizeNamedValue.Should().Be(MockParameterizeNamedValue);
            extractorParameters.ParameterizeApiLoggerId.Should().Be(MockParameterizeApiLoggerId);
            extractorParameters.ParameterizeLogResourceId.Should().Be(MockParameterizeLogResourceId);
            extractorParameters.NotIncludeNamedValue.Should().Be(MockNotIncludeNamedValue);
            extractorParameters.ParamNamedValuesKeyVaultSecrets.Should().Be(MockToParameterizeNamedValuesKeyVaultSecrets);
            extractorParameters.OperationBatchSize.Should().Be(MockOperationBatchSize);
            extractorParameters.ParameterizeBackend.Should().Be(MockParameterizeBackend);

            // more complicated assertions with parsing arguments
            extractorParameters.MultipleApiNames.Should().Contain(new[] { "test-multiple-api-1", "test-multiple-api-2" });

            extractorParameters.FileNames.Should().NotBeNull();
        }

        [Fact]
        public void ExtractorConfigValidate_MultipleApisNoStringPassed_DoesntThrowAnyException()
        {
            // arrange
            var defaultExtractorConfig = this.GetMockedExtractorConsoleAppConfiguration();
            defaultExtractorConfig.MultipleAPIs = string.Empty;

            // act
            Action createExtractorParameters = () => { _ = new ExtractorParameters(defaultExtractorConfig); };
            createExtractorParameters.Should().NotThrow();
        }
    }
}
