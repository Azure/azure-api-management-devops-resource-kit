using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using System;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor
{
    [Trait("Category", "Unit")]
    public class ExtractorConfigurationValidationTests
    {
        [Fact]
        public void ExtractorConfigValidate_NoPropertiesSet_MissingParameterException()
        {
            var extractorConfig = new ExtractorConsoleAppConfiguration();

            var expectedException = Assert.Throws<ArgumentException>(() => extractorConfig.Validate());

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

            extractorConfig.Validate();
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

            Assert.Throws<NotSupportedException>(() => extractorConfig.Validate());
        }
    }
}