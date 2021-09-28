using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract;
using System;
using Xunit;

namespace apimtemplate.test.Extractor
{
    [Trait("Category", "Unit")]
    public class ExtractorConfigTests
    {
        [Fact]
        public void ExtractorConfigValidate_NoPropertiesSet_MissingParameterException()
        {
            var extractorConfig = new ExtractorConfig();

            var expectedException = Assert.Throws<ArgumentException>(() => extractorConfig.Validate());

            Assert.Contains("Missing parameter", expectedException.Message);
        }

        [Fact]
        public void ExtractorConfigValidate_MinimumPropertiesSet_IsValid()
        {
            var extractorConfig = new ExtractorConfig
            {
                sourceApimName = "source-apim",
                destinationApimName = "destination-apim",
                resourceGroup = "resource-group",
                fileFolder = "c:/my/folder"
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
            var extractorConfig = new ExtractorConfig
            {
                sourceApimName = "source-apim",
                destinationApimName = "destination-apim",
                resourceGroup = "resource-group",
                fileFolder = "c:/my/folder",
                splitAPIs = splitApis,
                apiName = apiName,
                apiVersionSetName = apiVersionSetName,
                includeAllRevisions = includeAllRevisions,
                multipleAPIs = multipleApis
            };

            Assert.Throws<NotSupportedException>(() => extractorConfig.Validate());
        }
    }
}