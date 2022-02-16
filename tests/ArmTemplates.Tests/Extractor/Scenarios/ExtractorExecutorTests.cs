// --------------------------------------------------------------------------
//  <copyright file="ExtractorExecutorTests.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Mocked-Unit")]
    public class ExtractorExecutorTests : ExtractorTestsBase
    {
        readonly ExtractorParameters extractorParameters;
        readonly ExtractorExecutor extractorExecutor;

        public ExtractorExecutorTests()
        {
            var extractorConfig = this.GetDefaultExtractorConfiguration();
            
            this.extractorParameters = new ExtractorParameters(extractorConfig);
            this.extractorExecutor = new ExtractorExecutor(this.extractorParameters);
        }

        [Fact]
        public async Task RunDefaultExtraction_GeneratesProperFiles()
        {
            await this.extractorExecutor.ExecuteGenerationBasedOnConfiguration();
        }
    }
}
