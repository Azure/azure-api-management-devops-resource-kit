// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Configurations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Configuration
{
    [Trait("Category", "Unit")]
    public class ExtractorConfigurationOverrideTests : ExtractorMockerTestsBase
    {
        [Fact]
        public void ExtractorConfigValidate_NoPropertiesSet_MissingParameterException()
        {
            var defaultExtractorConfig = new ExtractorConsoleAppConfiguration
            {
                IncludeAllRevisions = true.ToString(),
                ParamApiLoggerId = true.ToString(),
                ParamBackend = true.ToString(),
                ParamLogResourceId = true.ToString(),
                ParamNamedValue = true.ToString(),
                ParamNamedValuesKeyVaultSecrets = true.ToString(),
                ParamServiceUrl = true.ToString(),
                NotIncludeNamedValue = true.ToString(),
                SplitAPIs = true.ToString(),
                ExtractGateways = true.ToString()
            };

            var extractorParameters = new ExtractorParameters(defaultExtractorConfig);
            extractorParameters = extractorParameters.OverrideConfiguration(new ExtractorConsoleAppConfiguration());

            extractorParameters.IncludeAllRevisions.Should().BeTrue();
            extractorParameters.ParameterizeServiceUrl.Should().BeTrue();
            extractorParameters.ParameterizeNamedValue.Should().BeTrue();
            extractorParameters.ParameterizeApiLoggerId.Should().BeTrue();
            extractorParameters.ParameterizeLogResourceId.Should().BeTrue();
            extractorParameters.NotIncludeNamedValue.Should().BeTrue();
            extractorParameters.ParamNamedValuesKeyVaultSecrets.Should().BeTrue();
            extractorParameters.ParameterizeBackend.Should().BeTrue();
            extractorParameters.SplitApis.Should().BeTrue();
            extractorParameters.IncludeAllRevisions.Should().BeTrue();
            extractorParameters.ExtractGateways.Should().BeTrue();
        }
    }
}
