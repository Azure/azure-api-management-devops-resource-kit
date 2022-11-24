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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Scenarios
{
    [Trait("Category", "Policy fragments Extraction")]
    public class PolicyFragmentsExtractorTests : ExtractorMockerWithOutputTestsBase
    {        
        public PolicyFragmentsExtractorTests() : base("policy-fragments-tests")
        {
        }

        [Fact]
        public async Task GeneratePolicyFragmentTemplates_ProperlyParsesResponse()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GeneratePolicyFragmentTemplates_ProperlyParsesResponse));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                apiName: "");
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var fileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListPolicyFragments_success_response.json");
            var mockedClient = await MockPolicyFragmentClient.GetMockedHttpPolicyFragmentClient(new MockClientConfiguration(responseFileLocation: fileLocation));
            var policyFragmentExtractor = new PolicyFragmentsExtractor(this.GetTestLogger<PolicyFragmentsExtractor>(), new TemplateBuilder(), mockedClient, null);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                policyFragmentsExtractor: policyFragmentExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var policyFragments = await extractorExecutor.GeneratePolicyFragmentsTemplateAsync(null, currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.PolicyFragments)).Should().BeTrue();

            policyFragments.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            policyFragments.TypedResources.PolicyFragments.Count().Should().Be(2);
            
            var policyFragment1 = policyFragments.TypedResources.PolicyFragments.First(x => x.OriginalName.Equals("policyFragment1"));
            policyFragment1.Should().NotBeNull();
            policyFragment1.Properties.Description.Should().Be("A policy fragment example 1");
            
            var policyFragment2 = policyFragments.TypedResources.PolicyFragments.First(x => x.OriginalName.Equals("policyFragment2"));
            policyFragment2.Should().NotBeNull();
            policyFragment2.Properties.Description.Should().Be("A policy fragment example 2");

        }

        [Fact]
        public async Task GeneratePolicyFragmentTemplates_GeneratesPolicyFragmentTemplateForSingleApi()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GeneratePolicyFragmentTemplates_GeneratesPolicyFragmentTemplateForSingleApi));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                apiName: "api");
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var fileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListPolicyFragments_success_response.json");
            var mockedClient = await MockPolicyFragmentClient.GetMockedHttpPolicyFragmentClient(new MockClientConfiguration(responseFileLocation: fileLocation));

            var policExtractor = new PolicyExtractor(this.GetTestLogger<PolicyExtractor>(), null, new TemplateBuilder());
            var policyFragmentExtractor = new PolicyFragmentsExtractor(this.GetTestLogger<PolicyFragmentsExtractor>(), new TemplateBuilder(), mockedClient, policExtractor);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                policyFragmentsExtractor: policyFragmentExtractor,
                policyExtractor: policExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            var apiPolicies = new List<PolicyTemplateResource>()
            {
                new PolicyTemplateResource()
                {
                    Properties = new PolicyTemplateProperties()
                    {
                        PolicyContent = "fragment-id=\"policyFragment1\""
                    }
                }
            };
            // act
            var policyFragments = await extractorExecutor.GeneratePolicyFragmentsTemplateAsync(apiPolicies, currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.PolicyFragments)).Should().BeTrue();

            policyFragments.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            policyFragments.TypedResources.PolicyFragments.Count().Should().Be(1);

            var policyFragment1 = policyFragments.TypedResources.PolicyFragments.First(x => x.OriginalName.Equals("policyFragment1"));
            policyFragment1.Should().NotBeNull();
            policyFragment1.Properties.Description.Should().Be("A policy fragment example 1");
        }

        [Fact]
        public async Task GeneratePolicyFragmentTemplates_GeneratesPolicyFragmentTemplateForSingleApi_GivenPolicyXmlBaseUrlParameter()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GeneratePolicyFragmentTemplates_GeneratesPolicyFragmentTemplateForSingleApi_GivenPolicyXmlBaseUrlParameter));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                policyXmlBaseUrl: "policyXmlBaseUrl",
                apiName: "api");
            var extractorParameters = new ExtractorParameters(extractorConfig);
            
            var policyFragmentResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListPolicyFragments_success_response.json");
            var mockedPolicyFragmentClient = await MockPolicyFragmentClient.GetMockedHttpPolicyFragmentClient(new MockClientConfiguration(responseFileLocation: policyFragmentResponseFileLocation));

            var policyResponseFileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementGetApiPolicy_success_response.json");
            var mockedPolicyClient = await MockPolicyClient.GetMockedHttpPolicyClient(new MockClientConfiguration(responseFileLocation: policyResponseFileLocation));

            var policExtractor = new PolicyExtractor(this.GetTestLogger<PolicyExtractor>(), mockedPolicyClient, new TemplateBuilder());
            var policyFragmentExtractor = new PolicyFragmentsExtractor(this.GetTestLogger<PolicyFragmentsExtractor>(), new TemplateBuilder(), mockedPolicyFragmentClient, policExtractor);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                policyFragmentsExtractor: policyFragmentExtractor,
                policyExtractor: policExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            // act
            var apiPolicies = await policExtractor.GenerateApiPolicyResourceAsync("api", currentTestDirectory, extractorParameters);
            var policyFragments = await extractorExecutor.GeneratePolicyFragmentsTemplateAsync(new List<PolicyTemplateResource> { apiPolicies }, currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.PolicyFragments)).Should().BeTrue();

            policyFragments.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            policyFragments.TypedResources.PolicyFragments.Count().Should().Be(1);

            var policyFragment1 = policyFragments.TypedResources.PolicyFragments.First(x => x.OriginalName.Equals("policyFragment1"));
            policyFragment1.Should().NotBeNull();
            policyFragment1.Properties.Description.Should().Be("A policy fragment example 1");
        }

        [Fact]
        public async Task GeneratePolicyFragmentTemplates_GeneratesPolicyFragmentTemplateForSingleApi_GivenDifferentPolicyFragmentIdCases()
        {
            // arrange
            var currentTestDirectory = Path.Combine(this.OutputDirectory, nameof(GeneratePolicyFragmentTemplates_GeneratesPolicyFragmentTemplateForSingleApi_GivenDifferentPolicyFragmentIdCases));

            var extractorConfig = this.GetDefaultExtractorConsoleAppConfiguration(
                apiName: "api");
            var extractorParameters = new ExtractorParameters(extractorConfig);
            var fileLocation = Path.Combine(MockClientUtils.ApiClientJsonResponsesPath, "ApiManagementListPolicyFragments_success_response.json");
            var mockedClient = await MockPolicyFragmentClient.GetMockedHttpPolicyFragmentClient(new MockClientConfiguration(responseFileLocation: fileLocation));

            var policExtractor = new PolicyExtractor(this.GetTestLogger<PolicyExtractor>(), null, new TemplateBuilder());
            var policyFragmentExtractor = new PolicyFragmentsExtractor(this.GetTestLogger<PolicyFragmentsExtractor>(), new TemplateBuilder(), mockedClient, policExtractor);

            var extractorExecutor = ExtractorExecutor.BuildExtractorExecutor(
                this.GetTestLogger<ExtractorExecutor>(),
                policyFragmentsExtractor: policyFragmentExtractor,
                policyExtractor: policExtractor);
            extractorExecutor.SetExtractorParameters(extractorParameters);

            var apiPolicies = new List<PolicyTemplateResource>()
            {
                new PolicyTemplateResource()
                {
                    Properties = new PolicyTemplateProperties()
                    {
                        PolicyContent = "fragment-id=\"POLICYFRAGMENT1\""
                    }
                }
            };
            // act
            var policyFragments = await extractorExecutor.GeneratePolicyFragmentsTemplateAsync(apiPolicies, currentTestDirectory);

            // assert
            File.Exists(Path.Combine(currentTestDirectory, extractorParameters.FileNames.PolicyFragments)).Should().BeTrue();

            policyFragments.Parameters.Should().ContainKey(ParameterNames.ApimServiceName);
            policyFragments.TypedResources.PolicyFragments.Count().Should().Be(1);

            var policyFragment1 = policyFragments.TypedResources.PolicyFragments.First(x => x.OriginalName.Equals("policyFragment1"));
            policyFragment1.Should().NotBeNull();
            policyFragment1.Properties.Description.Should().Be("A policy fragment example 1");
        }
    }
}
