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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients;
using Moq;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Authorization Server Extraction")]
    public class AuthorizationServerExtractorTests : ExtractorMockerWithOutputTestsBase
    {
        public AuthorizationServerExtractorTests() : base("authorization-server-tests")
        {
        }

        [Fact]
        public async Task GenerateAuthorizationServerTemplates_ProperlyLaysTheInformation()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateAuthorizationServerTemplates_ProperlyLaysTheInformation));

            var extractorConfig = this.GetMockedExtractorConsoleAppConfiguration(
                splitApis: false,
                apiVersionSetName: string.Empty,
                multipleApiNames: string.Empty,
                includeAllRevisions: false);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var mockedAuthorizationServerClient = MockAuthorizationServerClient.GetMockedApiClientWithDefaultValues();
            var authorizationServerExtractor = new AuthorizationServerExtractor(
                this.GetTestLogger<AuthorizationServerExtractor>(),
                new TemplateBuilder(),
                mockedAuthorizationServerClient);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                authorizationServerExtractor: authorizationServerExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var authorizationServerTemplate = await extractorExecutor.GenerateAuthorizationServerTemplateAsync(
                singleApiName: It.IsAny<string>(),
                currentTestDirectory,
                apiTemplateResources: It.IsAny<List<ApiTemplateResource>>());

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.AuthorizationServers)).Should().BeTrue();

            authorizationServerTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            authorizationServerTemplate.TypedResources.AuthorizationServers.Count().Should().Be(1);
            authorizationServerTemplate.Resources.Count().Should().Be(1);

            var authorizationResource = authorizationServerTemplate.TypedResources.AuthorizationServers.First();
            authorizationResource.Name.Should().Contain(MockAuthorizationServerClient.AuthorizationServerName1);
            authorizationResource.Type.Should().Contain(ResourceTypeConstants.AuthorizationServer);
            authorizationResource.Properties.Should().NotBeNull();

            authorizationResource.Properties.AuthorizationMethods.Any(x => x.Contains(MockAuthorizationServerClient.AuthorizationServerName1)).Should().BeTrue();
            authorizationResource.Properties.ClientAuthenticationMethod.Any(x => x.Contains(MockAuthorizationServerClient.AuthorizationServerName1)).Should().BeTrue();
            authorizationResource.Properties.DisplayName.Should().Contain(MockAuthorizationServerClient.AuthorizationServerName1);
            authorizationResource.Properties.TokenBodyParameters.Should().NotBeNull();
            authorizationResource.Properties.TokenEndpoint.Should().NotBeNull();
            authorizationResource.Properties.ClientRegistrationEndpoint.Should().NotBeNull();
            authorizationResource.Properties.AuthorizationEndpoint.Should().NotBeNull();
            authorizationResource.Properties.GrantTypes.Should().NotBeNullOrEmpty();
            authorizationResource.Properties.ClientId.Should().NotBeNullOrEmpty();
        }
    }
}
