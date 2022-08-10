// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients;
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
        public async Task GenerateSingleApiReleaseTemplateAsync_ProperlyCreatesTemplate()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateSingleApiReleaseTemplateAsync_ProperlyCreatesTemplate));

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

        [Fact]
        public async Task GenerateAllCurrentApiReleaseTemplateAsync_ProperlyCreatesTemplate()
        {
            // arrange
            var responseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListApis_success_response.json");
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateAllCurrentApiReleaseTemplateAsync_ProperlyCreatesTemplate));

            var mockedApiClientAllCurrent = await MockApisClient.GetMockedHttpApiClient(responseFileLocation);

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                apiName: string.Empty);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            // mocked extractors
            var apiReleaseExtractor = new ApiReleaseExtractor(this.GetTestLogger<ApiReleaseExtractor>(), new TemplateBuilder(), mockedApiClientAllCurrent);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                apiReleaseExtractor: apiReleaseExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var apiReleasesTemplate = await extractorExecutor.GenerateApiReleasesTemplateAsync(currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.ApiRelease)).Should().BeTrue();

            apiReleasesTemplate.TypedResources.ApiReleases.Count().Should().Be(4);
            apiReleasesTemplate.TypedResources.ApiReleases.All(x => x.Type.Equals(ResourceTypeConstants.APIRelease)).Should().BeTrue();

            apiReleasesTemplate.TypedResources.ApiReleases.Any(x => x.Properties.ApiId.Contains($"/apis/a1;rev=1")).Should().BeTrue();
            apiReleasesTemplate.TypedResources.ApiReleases.Any(x => x.Properties.ApiId.Contains($"/apis/echo-api;rev=1")).Should().BeTrue();
            apiReleasesTemplate.TypedResources.ApiReleases.Any(x => x.Properties.ApiId.Contains($"/apis/5a7390baa5816a110435aee0;rev=1")).Should().BeTrue();
            apiReleasesTemplate.TypedResources.ApiReleases.Any(x => x.Properties.ApiId.Contains($"/apis/5a73933b8f27f7cc82a2d533;rev=1")).Should().BeTrue();
        }
    }
}
