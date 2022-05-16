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
        public void TestOverrideName()
        {
            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(overrideGroupNames:"true");
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var groupTemplates = this.GetMockGroupTemplates();

            var groupDataProcessor = new GroupDataProcessor();

            groupDataProcessor.ProcessData(groupTemplates, extractorParameters);

            groupTemplates.ElementAt(0).Name.Should().BeEquivalentTo("administrators");
            groupTemplates.ElementAt(1).Name.Should().BeEquivalentTo("developers");
        }

        [Fact]
        public void TestSkipOverrideName()
        {
            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(overrideGroupNames: "false");
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var groupDataProcessor = new GroupDataProcessor();
            
            var groupTemplates = this.GetMockGroupTemplates();
            groupDataProcessor.ProcessData(groupTemplates, extractorParameters);

            groupTemplates.ElementAt(0).Name.Should().BeEquivalentTo("guid-administrators");
            groupTemplates.ElementAt(1).Name.Should().BeEquivalentTo("guid-developers");
        }
    }
}
