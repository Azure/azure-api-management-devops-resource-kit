using System.Collections.Generic;
using Xunit;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
{
    public class PolicyTemplateCreatorTests
    {
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
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{api.name}/policy')]", policyTemplateResource.name);
            Assert.Equal("rawxml-link", policyTemplateResource.properties.format);
            Assert.Equal(api.policy, policyTemplateResource.properties.value);
            Assert.Equal(dependsOn, policyTemplateResource.dependsOn);
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
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{apiName}/{policyPair.Key}/policy')]", policyTemplateResource.name);
            Assert.Equal("rawxml-link", policyTemplateResource.properties.format);
            Assert.Equal(policyPair.Value.policy, policyTemplateResource.properties.value);
            Assert.Equal(dependsOn, policyTemplateResource.dependsOn);
        }
    }
}
