// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Utilities
{
    public class ApiDataProcessorTest : ExtractorMockerTestsBase
    {
        public List<ApiTemplateResource> GetMockedApiTemplates() {
            return new List<ApiTemplateResource>
                {
                    new ApiTemplateResource
                    {
                        Name = "api1",
                        Type = ResourceTypeConstants.API,
                        Properties = new ApiProperties
                        {
                            DisplayName = "Unlimited",
                            Description = "unlimited description",
                            ApiRevision = "1",
                            IsCurrent = true,
                        }
                    },

                    new ApiTemplateResource
                    {
                        Name = "api1;rev=2",
                        Type = ResourceTypeConstants.API,
                        Properties = new ApiProperties
                        {
                            DisplayName = "Unlimited",
                            Description = "unlimited description",
                            ApiRevision = "2"
                        }
                    },
                };
        }

        [Fact]
        public void TestApiDataProcess()
        {
            var apiTemplates = this.GetMockedApiTemplates();

            var apiDataProcessor = new ApiDataProcessor();

            apiDataProcessor.ProcessData(apiTemplates);

            apiTemplates.ElementAt(0).Name.Should().BeEquivalentTo("api1;rev=1");
            apiTemplates.ElementAt(0).ApiNameWithRevision.Should().BeEquivalentTo("api1;rev=1");

            apiTemplates.ElementAt(1).Name.Should().BeEquivalentTo("api1;rev=2");
        }
    }
}
