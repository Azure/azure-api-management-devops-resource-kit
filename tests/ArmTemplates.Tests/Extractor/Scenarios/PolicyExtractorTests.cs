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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Policy Extraction")]
    public class PolicyExtractorTests : ExtractorMockerWithOutputTestsBase
    {
        public PolicyExtractorTests() : base("policy-tests")
        {
        }

        [Fact]
        public async Task GeneratePolicyTemplates_ProperlyLaysTheInformation()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GeneratePolicyTemplates_ProperlyLaysTheInformation));

            var extractorConfig = this.GetMockedExtractorConsoleAppConfiguration(
                splitApis: false,
                apiVersionSetName: string.Empty,
                multipleApiNames: string.Empty,
                includeAllRevisions: false);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var mockedPolicyApiClient = MockPolicyClient.GetMockedApiClientWithDefaultValues();
            var policyExtractor = new PolicyExtractor(this.GetTestLogger<PolicyExtractor>(), mockedPolicyApiClient, new TemplateBuilder());

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                policyExtractor: policyExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var policyTemplate = await extractorExecutor.GeneratePolicyTemplateAsync(currentTestDirectory);

            // policy template files exists
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.GlobalServicePolicy)).Should().BeTrue();
            // global service policy.xml
            File.Exists(Path.Combine(currentTestDirectory, PolicyExtractor.PoliciesDirectoryName, PolicyExtractor.GlobalServicePolicyFileName)).Should().BeTrue();

            policyTemplate.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            policyTemplate.Parameters.Should().ContainKey(ParameterNames.PolicyXMLBaseUrl);
            policyTemplate.Parameters.Should().ContainKey(ParameterNames.PolicyXMLSasToken);
            policyTemplate.TypedResources.Should().NotBeNull();
            policyTemplate.Resources.Count().Should().Be(1);

            var policyResource = policyTemplate.TypedResources.GlobalServicePolicy;
            policyResource.ApiVersion.Should().Be(GlobalConstants.ApiVersion);
            policyResource.Name.Should().NotBeNullOrEmpty();
            policyResource.Type.Should().Be(ResourceTypeConstants.GlobalServicePolicy);
            policyResource.Properties.Format.Should().NotBeNullOrEmpty();
            policyResource.Properties.PolicyContent.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GeneratePolicyTemplates_GetCachedPolicy_FoundAndReturnedCorrectly()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GeneratePolicyTemplates_GetCachedPolicy_FoundAndReturnedCorrectly));

            var extractorConfig = this.GetMockedExtractorConsoleAppConfiguration(
                splitApis: false,
                apiVersionSetName: string.Empty,
                multipleApiNames: string.Empty,
                includeAllRevisions: false);
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var mockedPolicyApiClient = MockPolicyClient.GetMockedApiClientWithDefaultValues();
            var policyExtractor = new PolicyExtractor(this.GetTestLogger<PolicyExtractor>(), mockedPolicyApiClient, new TemplateBuilder());

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                policyExtractor: policyExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);
            
            var policyTemplate = await extractorExecutor.GeneratePolicyTemplateAsync(currentTestDirectory);
            File.Exists(Path.Combine(currentTestDirectory, PolicyExtractor.PoliciesDirectoryName, PolicyExtractor.GlobalServicePolicyFileName)).Should().BeTrue();

            // try get global service policy from cache using full path of policy stored in object 
            var globalServicePolicyContent1 = policyExtractor.GetCachedPolicyContent(policyTemplate.TypedResources.GlobalServicePolicy, currentTestDirectory);
            globalServicePolicyContent1.Should().Be(MockPolicyClient.GlobalPolicyContent);

            // try get global service policy from cache using path retrieving
            policyTemplate.TypedResources.GlobalServicePolicy.Properties.PolicyContentFileFullPath = string.Empty;
            var globalServicePolicyContent2 = policyExtractor.GetCachedPolicyContent(policyTemplate.TypedResources.GlobalServicePolicy, currentTestDirectory);
            globalServicePolicyContent2.Should().Be(MockPolicyClient.GlobalPolicyContent);
        }

        [Fact]
        public async Task GenerateProductPolicyTemplateAsync_GivingResourceNotFound_ShouldReturnNull()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateProductPolicyTemplateAsync_GivingResourceNotFound_ShouldReturnNull));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(policyXmlBaseUrl: "policyXmlBaseUrl");
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var policyResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementGetProductPolicy_notfound_response.json");
            var mockedPolicyClient = await MockPolicyClient.GetMockedHttpPolicyClient(new MockClientConfiguration(responseFileLocation: policyResponseFileLocation, statusCode: System.Net.HttpStatusCode.NotFound));

            var policExtractor = new PolicyExtractor(this.GetTestLogger<PolicyExtractor>(), mockedPolicyClient, new TemplateBuilder());

            var productTemplateResource = new ProductsTemplateResource()
            {
                OriginalName = "productOriginalName",
            };
            
            // act
            var productPolicy = await policExtractor.GenerateProductPolicyTemplateAsync(extractorParameters, productTemplateResource, new string[] { }, currentTestDirectory);
            
            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.PolicyFragments)).Should().BeFalse();
            productPolicy.Should().BeNull();
        }

        [Fact]
        public async Task GenerateProductPolicyTemplateAsync_GivingResourceOkResult_ShouldReturnPolicy()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateProductPolicyTemplateAsync_GivingResourceOkResult_ShouldReturnPolicy));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(policyXmlBaseUrl: "policyXmlBaseUrl");
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var policyResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementGetProductPolicy_success_response.json");
            var mockedPolicyClient = await MockPolicyClient.GetMockedHttpPolicyClient(new MockClientConfiguration(responseFileLocation: policyResponseFileLocation));

            var policExtractor = new PolicyExtractor(this.GetTestLogger<PolicyExtractor>(), mockedPolicyClient, new TemplateBuilder());

            var productTemplateResource = new ProductsTemplateResource()
            {
                OriginalName = "productOriginalName",
                NewName = "productOriginalName"
            };

            // act
            var productPolicy = await policExtractor.GenerateProductPolicyTemplateAsync(extractorParameters, productTemplateResource, new string[] { }, currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, PolicyExtractor.PoliciesDirectoryName, string.Format(PolicyExtractor.ProductPolicyFileNameFormat, productTemplateResource.NewName))).Should().BeTrue();
            productPolicy.Should().NotBeNull();
            productPolicy.Properties.PolicyContent.Should().NotBeNull();
            productPolicy.Name.Should().EndWith("/policy')]");
        }

        [Fact]
        public async Task GenerateApiPolicyTemplateAsync_GivingResourceNotFound_ShouldReturnNull()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateApiPolicyTemplateAsync_GivingResourceNotFound_ShouldReturnNull));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(policyXmlBaseUrl: "policyXmlBaseUrl");
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var policyResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementGetApiPolicy_notfound_response.json");
            var mockedPolicyClient = await MockPolicyClient.GetMockedHttpPolicyClient(new MockClientConfiguration(responseFileLocation: policyResponseFileLocation, statusCode: System.Net.HttpStatusCode.NotFound));

            var policExtractor = new PolicyExtractor(this.GetTestLogger<PolicyExtractor>(), mockedPolicyClient, new TemplateBuilder());

            // act
            var apiPolicy = await policExtractor.GenerateApiPolicyResourceAsync("apiName", currentTestDirectory, extractorParameters);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, PolicyExtractor.PoliciesDirectoryName, string.Format(PolicyExtractor.ApiPolicyFileNameFormat, "apiName"))).Should().BeFalse();
            apiPolicy.Should().BeNull();
        }

        [Fact]
        public async Task GenerateApiPolicyTemplateAsync_GivingResourceOkResult_ShouldReturnPolicy()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateApiPolicyTemplateAsync_GivingResourceOkResult_ShouldReturnPolicy));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(policyXmlBaseUrl: "policyXmlBaseUrl");
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var policyResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementGetApiPolicy_success_response.json");
            var mockedPolicyClient = await MockPolicyClient.GetMockedHttpPolicyClient(new MockClientConfiguration(responseFileLocation: policyResponseFileLocation));

            var policExtractor = new PolicyExtractor(this.GetTestLogger<PolicyExtractor>(), mockedPolicyClient, new TemplateBuilder());

            // act
            var apiPolicy = await policExtractor.GenerateApiPolicyResourceAsync("apiName", currentTestDirectory, extractorParameters);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, PolicyExtractor.PoliciesDirectoryName, string.Format(PolicyExtractor.ApiPolicyFileNameFormat, "apiName"))).Should().BeTrue();
            apiPolicy.Should().NotBeNull();
            apiPolicy.Properties.PolicyContent.Should().NotBeNull();
            apiPolicy.Name.Should().EndWith("/policy')]");
        }

        [Fact]
        public async Task GenerateApiOperationPolicyTemplateAsync_GivingResourceNotFound_ShouldReturnNull()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateApiOperationPolicyTemplateAsync_GivingResourceNotFound_ShouldReturnNull));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(policyXmlBaseUrl: "policyXmlBaseUrl");
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var policyResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementGetApiOperationPolicy_notfound_response.json");
            var mockedPolicyClient = await MockPolicyClient.GetMockedHttpPolicyClient(new MockClientConfiguration(responseFileLocation: policyResponseFileLocation, statusCode: System.Net.HttpStatusCode.NotFound));

            var policExtractor = new PolicyExtractor(this.GetTestLogger<PolicyExtractor>(), mockedPolicyClient, new TemplateBuilder());

            // act
            var apiOperationPolicy = await policExtractor.GenerateApiOperationPolicyResourceAsync("apiName","operationName", currentTestDirectory, extractorParameters);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, PolicyExtractor.PoliciesDirectoryName, string.Format(PolicyExtractor.ApiOperationPolicyFileNameFormat, "apiName", "operationName"))).Should().BeFalse();
            apiOperationPolicy.Should().BeNull();
        }

        //next
        [Fact]
        public async Task GenerateApiOperationPolicyTemplateAsync_GivingResourceOkResult_ShouldReturnPolicy()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GenerateApiOperationPolicyTemplateAsync_GivingResourceOkResult_ShouldReturnPolicy));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(policyXmlBaseUrl: "policyXmlBaseUrl");
            var extractorParameters = new ExtractorParameters(extractorConfig);

            var policyResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementGetApiOperationPolicy_success_response.json");
            var mockedPolicyClient = await MockPolicyClient.GetMockedHttpPolicyClient(new MockClientConfiguration(responseFileLocation: policyResponseFileLocation));

            var policExtractor = new PolicyExtractor(this.GetTestLogger<PolicyExtractor>(), mockedPolicyClient, new TemplateBuilder());

            // act
            var apiPolicy = await policExtractor.GenerateApiOperationPolicyResourceAsync("apiName", "operationName", currentTestDirectory, extractorParameters);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, PolicyExtractor.PoliciesDirectoryName, string.Format(PolicyExtractor.ApiOperationPolicyFileNameFormat, "apiName", "operationName"))).Should().BeTrue();
            apiPolicy.Should().NotBeNull();
            apiPolicy.Properties.PolicyContent.Should().NotBeNull();
            apiPolicy.Name.Should().EndWith("/policy')]");
        }
    }
}
