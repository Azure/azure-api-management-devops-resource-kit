using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class PolicyTemplateCreator
    {
        private FileReader fileReader;
        CreatorConfig creatorConfig;

        public PolicyTemplateCreator(FileReader fileReader)
        {
            this.fileReader = fileReader;
        }
        public PolicyTemplateCreator(CreatorConfig creatorConfig)
        {
            this.creatorConfig = creatorConfig;
        }
        public async Task<PolicyTemplateResource> CreateAPIPolicyTemplateResourceAsync(CreatorConfig creatorConfig, string[] dependsOn)
        {
            // create policy resource with properties
            PolicyTemplateResource policyTemplateResource = new PolicyTemplateResource()
            {
                name = $"[concat(parameters('ApimServiceName'), '/{creatorConfig.api.name}/policy')]",
                type = "Microsoft.ApiManagement/service/apis/policies",
                apiVersion = "2018-01-01",
                properties = new PolicyTemplateProperties()
                {
                    contentFormat = "rawxml",
                    policyContent = creatorConfig.api.policy
                },
                dependsOn = dependsOn
            };
            return policyTemplateResource;
        }

        public async Task<PolicyTemplateResource> CreateOperationPolicyTemplateResourceAsync(KeyValuePair<string, OperationsConfig> policyPair, string apiName, string[] dependsOn)
        {
            // create policy resource with properties
            PolicyTemplateResource policyTemplateResource = new PolicyTemplateResource()
            {
                name = $"[concat(parameters('ApimServiceName'), '/{apiName}/{policyPair.Key}/policy')]",
                type = "Microsoft.ApiManagement/service/apis/operations/policies",
                apiVersion = "2018-01-01",
                properties = new PolicyTemplateProperties()
                {
                    contentFormat = "rawxml",
                    policyContent = policyPair.Value.policy
                },
                dependsOn = dependsOn
            };
            return policyTemplateResource;
        }

        public async Task<List<PolicyTemplateResource>> CreateOperationPolicyTemplateResourcesAsync(CreatorConfig creatorConfig, string[] dependsOn)
        {
            // create a policy resource for each policy listed in the config file and its associated provided xml file
            List<PolicyTemplateResource> policyTemplateResources = new List<PolicyTemplateResource>();
            foreach (KeyValuePair<string, OperationsConfig> pair in creatorConfig.api.operations)
            {
                policyTemplateResources.Add(await this.CreateOperationPolicyTemplateResourceAsync(pair, creatorConfig.api.name, dependsOn));
            }
            return policyTemplateResources;
        }
    }
}
