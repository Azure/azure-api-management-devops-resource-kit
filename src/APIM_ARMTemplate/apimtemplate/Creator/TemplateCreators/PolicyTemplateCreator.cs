using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class PolicyTemplateCreator
    {

        public async Task<PolicyTemplate> CreateAPIPolicyAsync(CreatorConfig creatorConfig)
        {
            FileReader fileReader = new FileReader();
            // create api schema with properties
            PolicyTemplate policyTemplate = new PolicyTemplate()
            {
                type = "Microsoft.ApiManagement/service/apis/policies",
                apiVersion = "2018-06-01-preview",
                properties = new PolicyTemplateProperties()
                {
                    contentFormat = "xml",
                    policyContent = await fileReader.RetrieveLocationContentsAsync(creatorConfig.api.policy)
                }
            };
            return policyTemplate;
        }

        public async Task<PolicyTemplate> CreateOperationPolicyAsync(KeyValuePair<string, OperationsConfig> policyPair)
        {
            FileReader fileReader = new FileReader();
            // create api schema with properties
            PolicyTemplate policyTemplate = new PolicyTemplate()
            {
                type = "Microsoft.ApiManagement/service/apis/operations/policies",
                apiVersion = "2018-06-01-preview",
                properties = new PolicyTemplateProperties()
                {
                    contentFormat = "xml",
                    policyContent = await fileReader.RetrieveLocationContentsAsync(policyPair.Value.policy)
                }
            };
            return policyTemplate;
        }

        public async Task<List<PolicyTemplate>> CreateOperationPolicies(CreatorConfig creatorConfig)
        {
            List<PolicyTemplate> policyTemplates = new List<PolicyTemplate>();
            foreach (KeyValuePair<string, OperationsConfig> pair in creatorConfig.api.operations)
            {
                policyTemplates.Add(await this.CreateOperationPolicyAsync(pair));
            }
            return policyTemplates;
        }
    }
}
