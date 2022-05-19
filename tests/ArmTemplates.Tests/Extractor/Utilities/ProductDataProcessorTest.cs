// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Utilities
{
    public class ProductDataProcessorTest: ExtractorMockerTestsBase
    {
        public List<ProductsTemplateResource> GetMockProductTemplates() {
            return new List<ProductsTemplateResource>
                {
                    new ProductsTemplateResource
                    {
                        Name = "unlimited-changed",
                        Type = ResourceTypeConstants.Product,
                        Properties = new ProductsProperties
                        {
                            DisplayName = "Unlimited",
                            Description = "unlimited description",
                        }
                    },

                    new ProductsTemplateResource
                    {
                        Name = "starter-changed",
                        Type = ResourceTypeConstants.Product,
                        Properties = new ProductsProperties
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

            var productTemplates = this.GetMockProductTemplates();

            var productDataProcessor = new ProductDataProcessor();

            productDataProcessor.ProcessData(productTemplates, extractorParameters);

            productTemplates.ElementAt(0).Name.Should().BeEquivalentTo("unlimited");
            productTemplates.ElementAt(1).Name.Should().BeEquivalentTo("starter");
        }

        [Fact]
        public void TestSkipOverrideName()
        {
            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(overrideProductGuids: "false");
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var productTemplates = this.GetMockProductTemplates();

            var productDataProcessor = new ProductDataProcessor();

            productDataProcessor.ProcessData(productTemplates, extractorParameters);

            productTemplates.ElementAt(0).Name.Should().BeEquivalentTo("unlimited-changed");
            productTemplates.ElementAt(1).Name.Should().BeEquivalentTo("starter-changed");
        }
    }
}
