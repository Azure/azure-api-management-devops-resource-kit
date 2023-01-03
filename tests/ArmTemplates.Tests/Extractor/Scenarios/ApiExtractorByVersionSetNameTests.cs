// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiManagementService;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiReleases;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiVersionSet;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.AuthorizationServer;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Groups;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.IdentityProviders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger.Cache;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.OpenIdConnectProviders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.PolicyFragments;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Schemas;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.TagApi;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients;
using Moq;
using Xunit;
using System.Linq;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Utils;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Api By Version Set Extraction")]
    public class ApiExtractorByVersionSetNameTests : ExtractorMockerWithOutputTestsBase
    {
        public ApiExtractorByVersionSetNameTests() : base("api-version-set-name-tests")
        {
        }

        [Fact]
        public async Task GenerateAPIVersionSetTemplates_RaisesError_GivenApiVersionSetName_DoesNotExist()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateAPIVersionSetTemplates_RaisesError_GivenApiVersionSetName_DoesNotExist));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                apiVersionSetName: "non-existing-api-version-set-name",
                fileFolder: currentTestDirectory);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var responseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListApis_success_response.json");
            var mockedApisClient = await MockApisClient.GetMockedHttpApiClient(new MockClientConfiguration(responseFileLocation: responseFileLocation));

            var apiClientUtils = new ApiClientUtils(mockedApisClient, this.GetTestLogger<ApiClientUtils>());

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                apiClientUtils: apiClientUtils);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act && assert
            Func<Task> act = () => extractorExecutor.GenerateAPIVersionSetTemplates();
            await act.Should().ThrowAsync<NoApiVersionSetWithSuchNameFoundException>().WithMessage(string.Format(ErrorMessages.ApiVersionDoesNotExistErrorMessage));
        }

        [Fact]
        public async Task GenerateAPIVersionSetTemplates_GeneratesApiTemplates()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateAPIVersionSetTemplates_GeneratesApiTemplates));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                apiVersionSetName: "api-version-set-name",
                fileFolder: currentTestDirectory);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var responseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListApis_ExpandVersionSet_success_response.json");
            var mockedApisClient = await MockApisClient.GetMockedHttpApiClient(new MockClientConfiguration(responseFileLocation: responseFileLocation));

            var apiClientUtils = new ApiClientUtils(mockedApisClient, this.GetTestLogger<ApiClientUtils>());

            var mockapiExtractor = new Mock<IApiExtractor>(MockBehavior.Strict);
            mockapiExtractor
                .Setup(x => x.GenerateApiTemplateAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new Template<ApiTemplateResources>()
                {
                    TypedResources = new ApiTemplateResources()
                });

            var mockedPolicyExtractor = new Mock<IPolicyExtractor>(MockBehavior.Strict);
            mockedPolicyExtractor
                .Setup(x => x.GenerateGlobalServicePolicyTemplateAsync(It.IsAny<ExtractorParameters>(), It.IsAny<string>()))
                .ReturnsAsync(new Template<PolicyTemplateResources>());

            var mockedProductApisExtractor = new Mock<IProductApisExtractor>(MockBehavior.Strict);
            mockedProductApisExtractor
                .Setup(x => x.GenerateProductApisTemplateAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new Template<ProductApiTemplateResources>());

            var mockedProductExtractor = new Mock<IProductExtractor>(MockBehavior.Strict);
            mockedProductExtractor
                .Setup(x => x.GenerateProductsTemplateAsync(It.IsAny<string>(), It.IsAny<List<ProductApiTemplateResource>>(), It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new Template<ProductTemplateResources>()
                {
                });

            var mockedApiVersionSetExtractor = new Mock<IApiVersionSetExtractor>(MockBehavior.Strict);
            mockedApiVersionSetExtractor
                .Setup(x => x.GenerateApiVersionSetTemplateAsync(It.IsAny<string>(), It.IsAny<List<ApiTemplateResource>>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new Template<ApiVersionSetTemplateResources>());

            var mockedAuthorizationServerExtractor = new Mock<IAuthorizationServerExtractor>(MockBehavior.Strict);
            mockedAuthorizationServerExtractor
                .Setup(x => x.GenerateAuthorizationServersTemplateAsync(It.IsAny<string>(), It.IsAny<List<ApiTemplateResource>>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new Template<AuthorizationServerTemplateResources>());

            var mockedTagsExtractor = new Mock<ITagExtractor>(MockBehavior.Strict);
            mockedTagsExtractor
                .Setup(x => x.GenerateTagsTemplateAsync(It.IsAny<string>(), It.IsAny<ApiTemplateResources>(), It.IsAny<ProductTemplateResources>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new Template<TagTemplateResources>());


            var mockedApiTagsExtractor = new Mock<ITagApiExtractor>(MockBehavior.Strict);
            mockedApiTagsExtractor
                .Setup(x => x.GenerateApiTagsTemplateAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new Template<TagApiTemplateResources>());

            var mockedLoggerExtractor = new Mock<ILoggerExtractor>(MockBehavior.Strict);
            mockedLoggerExtractor
                .Setup(x => x.GenerateLoggerTemplateAsync(It.IsAny<List<string>>(), It.IsAny<List<PolicyTemplateResource>>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new Template<LoggerTemplateResources>()
                {
                    TypedResources = new LoggerTemplateResources()
                });

            mockedLoggerExtractor
                .Setup(x => x.Cache)
                .Returns(new LoggersCache());

            var mockedNamedValuesExtractor = new Mock<INamedValuesExtractor>(MockBehavior.Strict);
            mockedNamedValuesExtractor
                .Setup(x => x.GenerateNamedValuesTemplateAsync(It.IsAny<string>(), It.IsAny<List<PolicyTemplateResource>>(), It.IsAny<List<LoggerTemplateResource>>(), It.IsAny<ExtractorParameters>(), It.IsAny<string>()))
                .ReturnsAsync(new Template<NamedValuesResources>()
                {
                    TypedResources = new NamedValuesResources()
                });

            var mockedBackendExtractor = new Mock<IBackendExtractor>(MockBehavior.Strict);
            mockedBackendExtractor
                .Setup(x => x.GenerateBackendsTemplateAsync(It.IsAny<string>(), It.IsAny<List<PolicyTemplateResource>>(), It.IsAny<string>(), It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new Template<BackendTemplateResources>()
                {
                    TypedResources = new BackendTemplateResources()
                });

            var mockedGroupExtractor = new Mock<IGroupExtractor>(MockBehavior.Strict);
            mockedGroupExtractor
                .Setup(x => x.GenerateGroupsTemplateAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new Template<GroupTemplateResources>());

            var mockedOpenIdConnectProvidersExtractor = new Mock<IOpenIdConnectProviderExtractor>(MockBehavior.Strict);
            mockedOpenIdConnectProvidersExtractor
                .Setup(x => x.GenerateOpenIdConnectProvidersTemplateAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new Template<OpenIdConnectProviderResources>()
                {
                    TypedResources = new OpenIdConnectProviderResources()
                });

            var mockedSchemasExtractor = new Mock<ISchemaExtractor>(MockBehavior.Strict);
            mockedSchemasExtractor
                .Setup(x => x.GenerateSchemasTemplateAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new Template<SchemaTemplateResources>());

            var mockedPolicyFragmentExtractor = new Mock<IPolicyFragmentsExtractor>(MockBehavior.Strict);
            mockedPolicyFragmentExtractor
                .Setup(x => x.GeneratePolicyFragmentsTemplateAsync(It.IsAny<List<PolicyTemplateResource>>(), It.IsAny<ExtractorParameters>(), It.IsAny<string>()))
                .ReturnsAsync(new Template<PolicyFragmentsResources>());

            var mockedApiReleasesExtractor = new Mock<IApiReleaseExtractor>(MockBehavior.Strict);
            mockedApiReleasesExtractor
                .Setup(x => x.GenerateCurrentApiReleaseTemplate(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new Template<ApiReleaseTemplateResources>());

            var mockedApimServiceExtractor = new Mock<IApiManagementServiceExtractor>(MockBehavior.Strict);
            mockedApimServiceExtractor
                .Setup(x => x.GenerateApiManagementServicesTemplateAsync(It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new Template<ApiManagementServiceResources>());

            var mockedParametersExtractor = new Mock<IParametersExtractor>(MockBehavior.Strict);
            mockedParametersExtractor
                .Setup(x => x.CreateMasterTemplateParameterValues(
                    It.IsAny<List<string>>(),
                    It.IsAny<LoggersCache>(),
                    It.IsAny<LoggerTemplateResources>(),
                    It.IsAny<BackendTemplateResources>(),
                    It.IsAny<NamedValuesResources>(),
                    It.IsAny<IdentityProviderResources>(),
                    It.IsAny<OpenIdConnectProviderResources>(),
                    It.IsAny<ExtractorParameters>()))
                .ReturnsAsync(new Template());

            mockedParametersExtractor
                .Setup(x => x.CreateResourceTemplateParameterTemplate(It.IsAny<Template>(), It.IsAny<Template>()))
                .Returns(new Template());

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                apisClient: mockedApisClient,
                apiExtractor: mockapiExtractor.Object,
                policyExtractor: mockedPolicyExtractor.Object,
                productApisExtractor: mockedProductApisExtractor.Object,
                productExtractor: mockedProductExtractor.Object,
                apiVersionSetExtractor: mockedApiVersionSetExtractor.Object,
                authorizationServerExtractor: mockedAuthorizationServerExtractor.Object,
                tagExtractor: mockedTagsExtractor.Object,
                tagApiExtractor: mockedApiTagsExtractor.Object,
                loggerExtractor: mockedLoggerExtractor.Object,
                namedValuesExtractor: mockedNamedValuesExtractor.Object,
                backendExtractor: mockedBackendExtractor.Object,
                groupExtractor: mockedGroupExtractor.Object,
                openIdConnectProviderExtractor: mockedOpenIdConnectProvidersExtractor.Object,
                schemaExtractor: mockedSchemasExtractor.Object,
                policyFragmentsExtractor: mockedPolicyFragmentExtractor.Object,
                apiReleaseExtractor: mockedApiReleasesExtractor.Object,
                parametersExtractor: mockedParametersExtractor.Object,
                apiManagementServiceExtractor: mockedApimServiceExtractor.Object,
                apiClientUtils: apiClientUtils
                );
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            await extractorExecutor.GenerateAPIVersionSetTemplates();

            // assert
            var directoriesCreated = Directory.GetDirectories(currentTestDirectory);
            directoriesCreated.Count().Should().Be(3);
            directoriesCreated.Any(x => x.StartsWith(Path.Combine(currentTestDirectory, "api-version-id-1"))).Should().BeTrue();
            directoriesCreated.Any(x => x.StartsWith(Path.Combine(currentTestDirectory, "api-version-id-2"))).Should().BeTrue();
            directoriesCreated.Any(x => x.Equals(Path.Combine(currentTestDirectory, extractorParameters.FileNames.VersionSetMasterFolder))).Should().BeTrue();
        }
    }
}
