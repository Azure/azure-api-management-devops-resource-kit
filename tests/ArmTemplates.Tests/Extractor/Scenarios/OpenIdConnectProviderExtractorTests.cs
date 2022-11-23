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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.IdentityProviderClients;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "OpenId connect providers Extraction")]
    public class OpenIdConnectProviderExtractorTests : ExtractorMockerWithOutputTestsBase
    {        
        public OpenIdConnectProviderExtractorTests() : base("open-id-connect-provider-tests")
        {
        }

        [Fact]
        public async Task GenerateOpenIdConnectProviderTemplates_ProperlyLaysTheInformation()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateOpenIdConnectProviderTemplates_ProperlyLaysTheInformation));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration();
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var openIdConnectProviderNames = new List<string>()
            {
                "open-id-connect-1",
                "open-id-connect-2"
            };

            var mockedOpenIdConnectProviderClient = MockOpenIdConnectProviderClient.GetMockedOpenIdConnectProviderClient(openIdConnectProviderNames);

            var openIdConnectProviderExtractor = new OpenIdConnectProviderExtractor(this.GetTestLogger<OpenIdConnectProviderExtractor>(), new TemplateBuilder(), mockedOpenIdConnectProviderClient);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                openIdConnectProviderExtractor: openIdConnectProviderExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var openIdConnectProviderTemplate = await extractorExecutor.GenerateOpenIdConnectProviderTemplateAsync(currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.OpenIdConnectProviders)).Should().BeTrue();

            openIdConnectProviderTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            openIdConnectProviderTemplate.Parameters.Should().ContainKey(ParameterNames.SecretValues);

            openIdConnectProviderTemplate.TypedResources.OpenIdConnectProviders.Count().Should().Be(2);
            openIdConnectProviderTemplate.Resources.Count().Should().Be(2);

            foreach (var templateResource in openIdConnectProviderTemplate.TypedResources.OpenIdConnectProviders)
            {
                templateResource.Should().NotBeNull();
                templateResource.Type.Should().Be(ResourceTypeConstants.OpenIdConnectProvider);
                templateResource.Properties.Should().NotBeNull();
                templateResource.Properties.ClientSecret.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task GenerateOpenIdConnectProviderTemplates_ProperlyParsesResponse()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateOpenIdConnectProviderTemplates_ProperlyParsesResponse));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration();
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var fileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListOpenIdConnectProviders_success_response.json");
            var mockedOpenIdConnectProviderClient = await MockOpenIdConnectProviderClient.GetMockedHttpOpenIdConnectProviderClient(new MockClientConfiguration(responseFileLocation: fileLocation));
            var openIdConnectExtractor = new OpenIdConnectProviderExtractor(this.GetTestLogger<OpenIdConnectProviderExtractor>(), new TemplateBuilder(), mockedOpenIdConnectProviderClient);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                openIdConnectProviderExtractor: openIdConnectExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var openIdConnectProviderTemplate = await extractorExecutor.GenerateOpenIdConnectProviderTemplateAsync(currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.OpenIdConnectProviders)).Should().BeTrue();

            openIdConnectProviderTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            openIdConnectProviderTemplate.Parameters.Should().ContainKey(ParameterNames.SecretValues);
            openIdConnectProviderTemplate.TypedResources.OpenIdConnectProviders.Count().Should().Be(2);
            
            var openIdConnectProvider1 = openIdConnectProviderTemplate.TypedResources.OpenIdConnectProviders.First(x => x.Properties.DisplayName.Equals("templateoidprovider1"));
            openIdConnectProvider1.Should().NotBeNull();
            openIdConnectProvider1.Properties.ClientSecret.Should().NotBeEmpty();

            var openIdConnectProvider2 = openIdConnectProviderTemplate.TypedResources.OpenIdConnectProviders.First(x => x.Properties.DisplayName.Equals("templateoidprovider2"));
            openIdConnectProvider2.Should().NotBeNull();
            openIdConnectProvider2.Properties.ClientSecret.Should().NotBeEmpty();
        }
    }
}
