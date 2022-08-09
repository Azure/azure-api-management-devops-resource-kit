// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Groups;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Utilities
{
    public class GroupDataProcessorTest: ExtractorMockerTestsBase
    {
        public List<GroupTemplateResource> GetMockGroupTemplates() {
            return new List<GroupTemplateResource>
                {
                    new GroupTemplateResource
                    {
                        Name = "guid-administrators",
                        Type = ResourceTypeConstants.Group,
                        Properties = new GroupProperties
                        {
                            DisplayName = "Administrators",
                            Description = "description",
                            BuiltIn = true,
                            Type = "system"
                        }
                    },

                    new GroupTemplateResource
                    {
                        Name = "guid-developers",
                        Type = ResourceTypeConstants.Group,
                        Properties = new GroupProperties
                        {
                            DisplayName = "Developers",
                            Description = "description",
                            BuiltIn = true,
                            Type = "system"
                        }
                    }
                };
        }

        [Fact]
        public void TestProcessDataAllGroups_OverrideName()
        {
            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(overrideGroupGuids: "true");
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var groupTemplates = this.GetMockGroupTemplates();

            var groupDataProcessor = new GroupDataProcessor();

            groupDataProcessor.ProcessDataAllGroups(groupTemplates, extractorParameters);

            groupTemplates.ElementAt(0).Name.Should().BeEquivalentTo("administrators");
            groupTemplates.ElementAt(1).Name.Should().BeEquivalentTo("developers");
        }

        [Fact]
        public void TestProcessDataAllGroups_SkipOverrideName()
        {
            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(overrideGroupGuids: "false");
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var groupDataProcessor = new GroupDataProcessor();
            
            var groupTemplates = this.GetMockGroupTemplates();
            groupDataProcessor.ProcessDataAllGroups(groupTemplates, extractorParameters);

            groupTemplates.ElementAt(0).Name.Should().BeEquivalentTo("guid-administrators");
            groupTemplates.ElementAt(1).Name.Should().BeEquivalentTo("guid-developers");
        }

        [Fact]
        public void TestProcessDataProductLinkedGroups_OverrideName()
        {
            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(overrideGroupGuids: "true");
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var groupTemplates = this.GetMockGroupTemplates();

            var groupDataProcessor = new GroupDataProcessor();

            groupDataProcessor.ProcessDataAllGroups(groupTemplates, extractorParameters);

            groupTemplates.ElementAt(0).Name.Should().BeEquivalentTo("administrators");
            groupTemplates.ElementAt(1).Name.Should().BeEquivalentTo("developers");
        }

        [Fact]
        public void TestProcessDataProductLinkedGroups_SkipOverrideName()
        {
            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(overrideGroupGuids: "false");
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var groupDataProcessor = new GroupDataProcessor();

            var groupTemplates = this.GetMockGroupTemplates();
            groupDataProcessor.ProcessDataAllGroups(groupTemplates, extractorParameters);

            groupTemplates.ElementAt(0).Name.Should().BeEquivalentTo("guid-administrators");
            groupTemplates.ElementAt(1).Name.Should().BeEquivalentTo("guid-developers");
        }

        [Fact]
        public void TestExcludeBuiltInGroups()
        {
            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(excludeBuildInGroups: "true");
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var groupDataProcessor = new GroupDataProcessor();

            var groupTemplates = this.GetMockGroupTemplates();
            groupDataProcessor.ProcessDataAllGroups(groupTemplates, extractorParameters);

            groupTemplates.Count.Should().Be(0);
        }
    }
}
