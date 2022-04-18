// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Configurations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using System;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Configuration
{
    [Trait("Category", "Unit")]
    public class ExtractorConfigurationValidationTests
    {
        [Fact]
        public void ExtractorConfigValidate_NoPropertiesSet_MissingParameterException()
        {
            var extractorConfig = new ExtractorConsoleAppConfiguration();
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var expectedException = Assert.Throws<ArgumentException>(() => extractorParameters.Validate());

            Assert.Contains("Missing parameter", expectedException.Message);
        }

        [Fact]
        public void ExtractorConfigValidate_MinimumPropertiesSet_IsValid()
        {
            var extractorConfig = new ExtractorConsoleAppConfiguration
            {
                SourceApimName = "source-apim",
                DestinationApimName = "destination-apim",
                ResourceGroup = "resource-group",
                FileFolder = "c:/my/folder"
            };
            var extractorParameters = new ExtractorParameters(extractorConfig);

            extractorParameters.Validate();
        }

        [Theory]
        [InlineData("true", null, "my-api", null, null)]
        [InlineData("true", "api-version-set", null, null, null)]
        [InlineData(null, "api-version-set", null, null, "true")]
        [InlineData(null, null, "my-api", null, "true")]
        [InlineData(null, "api-version-set", "my-api", null, null)]
        [InlineData(null, null, null, "true", null)]
        public void ExtractorConfigValidate_VerifyNotSupportedCases_ThrowsException(string splitApis, string apiVersionSetName, string apiName, string includeAllRevisions, string multipleApis)
        {
            var extractorConfig = new ExtractorConsoleAppConfiguration
            {
                SourceApimName = "source-apim",
                DestinationApimName = "destination-apim",
                ResourceGroup = "resource-group",
                FileFolder = "c:/my/folder",
                SplitAPIs = splitApis,
                ApiName = apiName,
                ApiVersionSetName = apiVersionSetName,
                IncludeAllRevisions = includeAllRevisions,
                MultipleAPIs = multipleApis
            };
            var extractorParameters = new ExtractorParameters(extractorConfig);

            Assert.Throws<NotSupportedException>(() => extractorParameters.Validate());
        }
    }
}