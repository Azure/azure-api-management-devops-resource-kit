using System.Collections.Generic;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class PolicyTemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateAPIPolicyTemplateResourceFromCreatorConfigWithCorrectContent()
        {
            // arrange
            PolicyTemplateCreator policyTemplateCreator = PolicyTemplateCreatorFactory.GeneratePolicyTemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig()
            {
                api = new APIConfig()
                {
                    name = "name",
                    policy = "http://someurl.com"
                }
            };
            string[] dependsOn = new string[] { "dependsOn" };


            // act
            PolicyTemplateResource policyTemplateResource = policyTemplateCreator.CreateAPIPolicyTemplateResource(creatorConfig, dependsOn);

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{creatorConfig.api.name}/policy')]", policyTemplateResource.name);
            Assert.Equal("rawxml-link", policyTemplateResource.properties.contentFormat);
            Assert.Equal(creatorConfig.api.policy, policyTemplateResource.properties.policyContent);
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
            Assert.Equal("rawxml-link", policyTemplateResource.properties.contentFormat);
            Assert.Equal(policyPair.Value.policy, policyTemplateResource.properties.policyContent);
            Assert.Equal(dependsOn, policyTemplateResource.dependsOn);
        }
    }
}
