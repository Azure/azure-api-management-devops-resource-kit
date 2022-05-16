// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Utilities
{
    public class ProductApiDataProcessorTest: ExtractorMockerTestsBase
    {
        public List<ProductApiTemplateResource> GetMockProductApiTemplates() {
            return new List<ProductApiTemplateResource>
                {
                    new ProductApiTemplateResource
                    {
                        Name = "unlimited-changed",
                        Type = ResourceTypeConstants.ProductApi,
                        Properties = new ProductApiProperties
                        {
                            DisplayName = "Unlimited",
                            Description = "unlimited description",
                        }
                    },

                    new ProductApiTemplateResource
                    {
                        Name = "starter-changed",
                        Type = ResourceTypeConstants.ProductApi,
                        Properties = new ProductApiProperties
                        {
                            DisplayName = "Starter",
                            Description = "starter description",
                        }
                    }
                };
        }

        [Fact]
        public void TestOverrideName()
        {
            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(overrideProductGuids: "true");
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var productApiTemplates = this.GetMockProductApiTemplates();

            var productApiDataProcessor = new ProductApiDataProcessor();

            productApiDataProcessor.ProcessData(productApiTemplates, extractorParameters);

            productApiTemplates.ElementAt(0).Name.Should().BeEquivalentTo("unlimited");
            productApiTemplates.ElementAt(1).Name.Should().BeEquivalentTo("starter");
        }

        [Fact]
        public void TestSkipOverrideName()
        {
            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(overrideProductGuids: "false");
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var productApiTemplates = this.GetMockProductApiTemplates();

            var productApiDataProcessor = new ProductApiDataProcessor();

            productApiDataProcessor.ProcessData(productApiTemplates, extractorParameters);

            productApiTemplates.ElementAt(0).Name.Should().BeEquivalentTo("unlimited-changed");
            productApiTemplates.ElementAt(1).Name.Should().BeEquivalentTo("starter-changed");
        }
    }
}
