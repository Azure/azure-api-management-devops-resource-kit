using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class FileNameGenerator
    {
        public CreatorFileNames GenerateCreatorFileNames(Template apiVersionSetTemplate,
            Template initialAPITemplate,
            Template subsequentAPITemplate,
            List<Template> productAPITemplates,
            Template apiPolicyTemplate,
            List<Template> operationPolicyTemplates)
        {
            Dictionary<string, string> operationPolicies = new Dictionary<string, string>();
            Dictionary<string, string> productAPIs = new Dictionary<string, string>();
            foreach (Template operationPolicyTemplate in operationPolicyTemplates)
            {
                // outputs { "operationpolicy-getSessions" : "/operationpolicy-getSessions.template.json"}
                operationPolicies.Add(operationPolicyTemplate.resources[0].name, $@"/{operationPolicyTemplate.resources[0].name}.template.json");
            }
            foreach (Template productAPITemplate in productAPITemplates)
            {
                // outputs { "apiproduct-starter" : "/apiproduct-starter.template.json"}
                productAPIs.Add(productAPITemplate.resources[0].name, $@"/{productAPITemplate.resources[0].name}.template.json");
            }
            return new CreatorFileNames()
            {
                apiVersionSet = $@"/{apiVersionSetTemplate.resources[0].name}.template.json",
                initialAPI = $@"/{initialAPITemplate.resources[0].name}-initial.template.json",
                subsequentAPI = $@"/{subsequentAPITemplate.resources[0].name}-subsequent.template.json",
                apiPolicy = $@"/{apiPolicyTemplate.resources[0].name}.template.json",
                operationPolicies = operationPolicies,
                productAPIs = productAPIs,
                master = @"/master.template.json"
            };
        }

    }
}
