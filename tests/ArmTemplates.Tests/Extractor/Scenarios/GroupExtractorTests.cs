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
    }
}
