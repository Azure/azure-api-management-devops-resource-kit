// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Xunit;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorFactories;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class PolicyTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateGlobalServicePolicyTemplateResourceFromCreatorConfigWithCorrectContent()
        {
            // arrange
            PolicyTemplateCreator policyTemplateCreator = PolicyTemplateCreatorFactory.GeneratePolicyTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { policy = "http://someurl.com" };

            // act
            var policyTemplate = policyTemplateCreator.CreateGlobalServicePolicyTemplate(creatorConfig);
            var policyTemplateResource = policyTemplate.Resources[0] as PolicyTemplateResource;

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/policy')]", policyTemplateResource.Name);
            Assert.Equal("rawxml-link", policyTemplateResource.Properties.Format);
            Assert.Equal(creatorConfig.policy, policyTemplateResource.Properties.PolicyContent);
        }

        [Fact]
        public void ShouldCreateAPIPolicyTemplateResourceFromCreatorConfigWithCorrectContent()
        {
            // arrange
            PolicyTemplateCreator policyTemplateCreator = PolicyTemplateCreatorFactory.GeneratePolicyTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { apis = new List<APIConfig>() };
            APIConfig api = new APIConfig()
            {
                name = "name",
                policy = "http://someurl.com"
            };
            creatorConfig.apis.Add(api);
            string[] dependsOn = new string[] { "dependsOn" };

            // act
            PolicyTemplateResource policyTemplateResource = policyTemplateCreator.CreateAPIPolicyTemplateResource(api, dependsOn);

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.name}/policy')]", policyTemplateResource.Name);
            Assert.Equal("rawxml-link", policyTemplateResource.Properties.Format);
            Assert.Equal(api.policy, policyTemplateResource.Properties.PolicyContent);
            Assert.Equal(dependsOn, policyTemplateResource.DependsOn);
        }

        [Fact]
        public void ShouldCreateProductPolicyTemplateResourceFromCreatorConfigWithCorrectContent()
        {
            // arrange
            PolicyTemplateCreator policyTemplateCreator = PolicyTemplateCreatorFactory.GeneratePolicyTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { products = new List<ProductConfig>() };
            ProductConfig product = new ProductConfig()
            {
                DisplayName = "displayName",
                Description = "description",
                Terms = "terms",
                SubscriptionRequired = true,
                ApprovalRequired = true,
                SubscriptionsLimit = 1,
                State = "state",
                policy = "http://someurl.com"

            };
            creatorConfig.products.Add(product);
            string[] dependsOn = new string[] { "dependsOn" };

            // act
            PolicyTemplateResource policyTemplateResource = policyTemplateCreator.CreateProductPolicyTemplateResource(product, dependsOn);

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{product.DisplayName}/policy')]", policyTemplateResource.Name);
            Assert.Equal("rawxml-link", policyTemplateResource.Properties.Format);
            Assert.Equal(product.policy, policyTemplateResource.Properties.PolicyContent);
            Assert.Equal(dependsOn, policyTemplateResource.DependsOn);
        }

        [Fact]
        public void ShouldCreateOperationPolicyTemplateResourceFromPairWithCorrectContent()
        {
            // arrange
            PolicyTemplateCreator policyTemplateCreator = PolicyTemplateCreatorFactory.GeneratePolicyTemplateCreator();
            KeyValuePair<string, OperationsConfig> policyPair = new KeyValuePair<string, OperationsConfig>("key", new OperationsConfig() { policy = "http://someurl.com" });
            string apiName = "apiName";
            string[] dependsOn = new string[] { "dependsOn" };

            // act
            PolicyTemplateResource policyTemplateResource = policyTemplateCreator.CreateOperationPolicyTemplateResource(policyPair, apiName, dependsOn);

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{policyPair.Key}/policy')]", policyTemplateResource.Name);
            Assert.Equal("rawxml-link", policyTemplateResource.Properties.Format);
            Assert.Equal(policyPair.Value.policy, policyTemplateResource.Properties.PolicyContent);
            Assert.Equal(dependsOn, policyTemplateResource.DependsOn);
        }
    }
}
