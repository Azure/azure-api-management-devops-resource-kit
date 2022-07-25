// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Api Release Extraction")]
    public class ApiReleaseExtractorTests : ExtractorMockerWithOutputTestsBase
    {
        public ApiReleaseExtractorTests() : base("api-release-tests")
        {
        }

        [Fact]
        public async Task GenerateApiReleaseTemplateAsync_ProperlyCreatesTemplate()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateApiReleaseTemplateAsync_ProperlyCreatesTemplate));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration();
            var extractorParameters = new ExtractorParameters(extractorConfig);

            // mocked extractors
            var apiReleaseExtractor = new ApiReleaseExtractor(this.GetTestLogger<ApiReleaseExtractor>(), new TemplateBuilder(), null);
            
            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                apiReleaseExtractor: apiReleaseExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            var apiId = "api1;rev=2";

            // act
            var apiReleaseTemplate = await extractorExecutor.GenerateApiReleaseTemplateAsync(apiId, currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.ApiRelease)).Should().BeTrue();

            apiReleaseTemplate.TypedResources.ApiReleases.Count().Should().Be(1);
            var apiRelease = apiReleaseTemplate.TypedResources.ApiReleases[0];

            apiRelease.Properties.Should().NotBeNull();
            apiRelease.Properties.ApiId.Should().Be($"/apis/{apiId}");
        }
    }
}
