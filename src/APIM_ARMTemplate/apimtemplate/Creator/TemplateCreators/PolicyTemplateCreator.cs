using System.IO;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class PolicyTemplateCreator
    {
        private FileReader fileReader;

        public PolicyTemplateCreator(FileReader fileReader)
        {
            this.fileReader = fileReader;
        }

        public async Task<PolicyTemplateResource> CreateAPIPolicyTemplateResourceAsync(CreatorConfig creatorConfig, string[] dependsOn)
        {
            // create policy resource with properties
            PolicyTemplateResource policyTemplateResource = new PolicyTemplateResource()
            {
                name = $"[concat(parameters('ApimServiceName'), '/{creatorConfig.api.name}/policy')]",
                type = "Microsoft.ApiManagement/service/apis/policies",
                apiVersion = "2018-06-01-preview",
                properties = new PolicyTemplateProperties()
                {
                    contentFormat = "rawxml",
                    policyContent = await this.fileReader.RetrieveLocationContentsAsync(creatorConfig.api.policy)
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
                apiVersion = "2018-06-01-preview",
                properties = new PolicyTemplateProperties()
                {
                    contentFormat = "rawxml",
                    policyContent = await this.fileReader.RetrieveLocationContentsAsync(policyPair.Value.policy)
                },
                dependsOn = dependsOn
            };
            return policyTemplateResource;
        }

        public async Task<List<PolicyTemplateResource>> CreateOperationPolicyTemplateResourcesAsync(CreatorConfig creatorConfig, string[] dependsOn)
        {
            List<PolicyTemplateResource> policyTemplateResources = new List<PolicyTemplateResource>();
            foreach (KeyValuePair<string, OperationsConfig> pair in creatorConfig.api.operations)
            {
                policyTemplateResources.Add(await this.CreateOperationPolicyTemplateResourceAsync(pair, creatorConfig.api.name, dependsOn));
            }
            return policyTemplateResources;
        }
    }
}
