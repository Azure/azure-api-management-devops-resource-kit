﻿// --------------------------------------------------------------------------
//  <copyright file="PolicyExtractorTests.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

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
    }
}
