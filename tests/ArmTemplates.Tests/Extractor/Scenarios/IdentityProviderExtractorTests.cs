// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.IdentityProviderClients;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "IdentityProvider Extraction")]
    public class IdentityProviderExtractorTests : ExtractorMockerWithOutputTestsBase
    {        
        public IdentityProviderExtractorTests() : base("identity-provider-tests")
        {
        }

        [Fact]
        public async Task GenerateIdentityProviderTemplates_ProperlyLaysTheInformation()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateIdentityProviderTemplates_ProperlyLaysTheInformation));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration();
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var identityProviderNames = new List<string>()
            {
                "identityProvider1",
                "identityProvider2"
            };

            var mockedIdentityProviderClient = MockIdentityProviderClient.GetMockedIdentityProviderClient(identityProviderNames);

            var identityProviderExtractor = new IdentityProviderExtractor(this.GetTestLogger<IdentityProviderExtractor>(), new TemplateBuilder(), mockedIdentityProviderClient);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                identityProviderExtractor: identityProviderExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var identityProviderTemplate = await extractorExecutor.GenerateIdentityProviderTemplateAsync(currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.IdentityProviders)).Should().BeTrue();

            identityProviderTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            identityProviderTemplate.Parameters.Should().ContainKey(ParameterNames.SecretValues);

            identityProviderTemplate.TypedResources.IdentityProviders.Count().Should().Be(2);
            identityProviderTemplate.Resources.Count().Should().Be(2);

            foreach (var templateResource in identityProviderTemplate.TypedResources.IdentityProviders)
            {
                templateResource.Should().NotBeNull();
                templateResource.Type.Should().Be(ResourceTypeConstants.IdentityProviders);
                templateResource.Properties.Should().NotBeNull();
                templateResource.Properties.ClientSecret.Should().NotBeNull();
            }
        }
    }
}
