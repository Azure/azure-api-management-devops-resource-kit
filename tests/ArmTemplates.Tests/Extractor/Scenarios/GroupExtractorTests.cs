// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Groups Extraction")]
    public class GroupExtractorTests : ExtractorMockerWithOutputTestsBase
    {        
        public GroupExtractorTests() : base("groups-tests")
        {
        }

        [Fact]
        public async Task GenerateGroupsTemplates_ProperlyLaysTheInformation()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateGroupsTemplates_ProperlyLaysTheInformation));

            var extractorConfig = this.GetMockedExtractorConsoleAppConfiguration(
                splitApis: false,
                apiVersionSetName: string.Empty,
                multipleApiNames: string.Empty,
                includeAllRevisions: false);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var mockedGroupsClient = MockGroupsClient.GetMockedApiClientWithDefaultValues();
            var groupExtractor = new GroupExtractor(this.GetTestLogger<GroupExtractor>(), new TemplateBuilder(), mockedGroupsClient);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                groupExtractor: groupExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var groupTemplate = await extractorExecutor.GenerateGroupsTemplateAsync(currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Groups)).Should().BeTrue();

            groupTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            groupTemplate.TypedResources.Groups.Count().Should().Be(2);
            groupTemplate.Resources.Count().Should().Be(2);

            (groupTemplate.Resources[0].Name.Contains(MockGroupsClient.GroupName1) || groupTemplate.Resources[1].Name.Contains(MockGroupsClient.GroupName1)).Should().BeTrue();
            (groupTemplate.Resources[0].Name.Contains(MockGroupsClient.GroupName2) || groupTemplate.Resources[1].Name.Contains(MockGroupsClient.GroupName2)).Should().BeTrue();

            foreach (var templateResource in groupTemplate.TypedResources.Groups)
            {
                templateResource.Type.Should().Be(ResourceTypeConstants.Group);
                templateResource.Should().NotBeNull();
                templateResource.Properties.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task GenerateGroupsTemplates_ProperlyParsesResponse()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateGroupsTemplates_ProperlyParsesResponse));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration();
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var fileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListGroups_success_response.json");
            var mockedGroupsClient = await MockGroupsClient.GetMockedHttpGroupClient(new MockClientConfiguration(responseFileLocation: fileLocation));
            var groupExtractor = new GroupExtractor(this.GetTestLogger<GroupExtractor>(), new TemplateBuilder(), mockedGroupsClient);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                groupExtractor: groupExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var groupTemplate = await extractorExecutor.GenerateGroupsTemplateAsync(currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Groups)).Should().BeTrue();

            groupTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            groupTemplate.TypedResources.Groups.Count().Should().Be(4);
            groupTemplate.TypedResources.Groups.First(x => x.Properties.DisplayName.Equals("AwesomeGroup for test")).Should().NotBeNull();
        }

        [Fact]
        public async Task GenerateGroupsTemplates_DoesNotGenerateTemplate_GivenGroupListMethod_IsNotAllowed()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateGroupsTemplates_DoesNotGenerateTemplate_GivenGroupListMethod_IsNotAllowed));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                apiName: null);
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var fileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagement_methodnotallowed_response.json");
            var mockedGroupsClient = await MockGroupsClient.GetMockedHttpGroupClient(new MockClientConfiguration(responseFileLocation: fileLocation, statusCode: System.Net.HttpStatusCode.BadRequest)) ;
            var groupExtractor = new GroupExtractor(this.GetTestLogger<GroupExtractor>(), new TemplateBuilder(), mockedGroupsClient);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                groupExtractor: groupExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var groupTemplate = await extractorExecutor.GenerateGroupsTemplateAsync(currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.Groups)).Should().BeFalse();
        }

        [Fact]
        public async Task GenerateGroupsTemplates_RaisesError_GivenGroupListMethod_ReturnsNotFound()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateGroupsTemplates_RaisesError_GivenGroupListMethod_ReturnsNotFound));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                apiName: null);
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var fileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagement_notfound_response.json");
            var mockedGroupsClient = await MockGroupsClient.GetMockedHttpGroupClient(new MockClientConfiguration(responseFileLocation: fileLocation, statusCode: System.Net.HttpStatusCode.NotFound));
            var groupExtractor = new GroupExtractor(this.GetTestLogger<GroupExtractor>(), new TemplateBuilder(), mockedGroupsClient);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                groupExtractor: groupExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act & assert
            var asyncFunction = async () =>
            {
                await extractorExecutor.GenerateGroupsTemplateAsync(currentTestDirectory);
            };
            
            var results = await asyncFunction.Should().ThrowAsync<HttpRequestException>().Where(e => e.Message.Contains("404"));
        }
    }
}
