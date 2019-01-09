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
            List<Template> operationPolicyTemplates,
            CreatorConfig creatorConfig)
        {
            Dictionary<string, string> operationPolicies = new Dictionary<string, string>();
            Dictionary<string, string> productAPIs = new Dictionary<string, string>();
            foreach (Template operationPolicyTemplate in operationPolicyTemplates)
            {
                // outputs { "getSessions" : "/operationpolicy-getSessions.template.json"}
                KeyValuePair<string, string> pair = this.RetrieveFileNameForOperationPolicyTemplate(operationPolicyTemplate, creatorConfig);
                operationPolicies.Add(pair.Key, pair.Value);
            }
            foreach (Template productAPITemplate in productAPITemplates)
            {
                // outputs { "starter" : "/productapi-starter.template.json"}
                KeyValuePair<string, string> pair = this.RetrieveFileNameForProductAPITemplate(productAPITemplate, creatorConfig);
                productAPIs.Add(pair.Key, pair.Value);
            }
            return new CreatorFileNames()
            {
                apiVersionSet = $@"/versionset.template.json",
                initialAPI = $@"/api-initial.template.json",
                subsequentAPI = $@"/api-subsequent.template.json",
                apiPolicy = $@"/apipolicy.template.json",
                operationPolicies = operationPolicies,
                productAPIs = productAPIs,
                master = @"/master.template.json"
            };
        }

        public KeyValuePair<string, string> RetrieveFileNameForOperationPolicyTemplate(Template operationPolicyTemplate, CreatorConfig creatorConfig)
        {
            foreach (string operation in creatorConfig.api.operations.Keys)
            {
                if (operationPolicyTemplate.resources[0].name.Contains(operation))
                {
                    return new KeyValuePair<string, string>(operation, $@"/operationpolicy-{operation}.template.json");
                };
            };
            return new KeyValuePair<string, string>("", "");
        }

        public KeyValuePair<string, string> RetrieveFileNameForProductAPITemplate(Template productAPITemplate, CreatorConfig creatorConfig)
        {
            string[] productIDs = creatorConfig.api.products.Split(", ");
            foreach (string productID in productIDs)
            {
                if (productAPITemplate.resources[0].name.Contains(productID))
                {
                    return new KeyValuePair<string, string>(productID, $@"/productapi-{productID}.template.json");
                };
            };
            return new KeyValuePair<string, string>("", "");
        }

    }
}
