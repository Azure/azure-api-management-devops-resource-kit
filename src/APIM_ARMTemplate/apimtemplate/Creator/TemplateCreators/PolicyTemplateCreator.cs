using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class PolicyTemplateCreator
    {
        private FileReader fileReader;

        public PolicyTemplateCreator(FileReader fileReader)
        {
            this.fileReader = fileReader;
        }

        public PolicyTemplateResource CreateAPIPolicyTemplateResource(CreatorConfig creatorConfig, string[] dependsOn)
        {
            Uri uriResult;
            bool isUrl = Uri.TryCreate(creatorConfig.api.policy, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            // create policy resource with properties
            PolicyTemplateResource policyTemplateResource = new PolicyTemplateResource()
            {
                name = $"[concat(parameters('ApimServiceName'), '/{creatorConfig.api.name}/policy')]",
                type = ResourceTypeConstants.APIPolicy,
                apiVersion = "2018-06-01-preview",
                properties = new PolicyTemplateProperties()
                {
                    contentFormat = isUrl ? "rawxml-link" : "rawxml",
                    policyContent = isUrl ? creatorConfig.api.policy : this.fileReader.RetrieveLocalFileContents(creatorConfig.api.policy)
                },
                dependsOn = dependsOn
            };
            return policyTemplateResource;
        }

        public PolicyTemplateResource CreateOperationPolicyTemplateResource(KeyValuePair<string, OperationsConfig> policyPair, string apiName, string[] dependsOn)
        {
            Uri uriResult;
            bool isUrl = Uri.TryCreate(policyPair.Value.policy, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            // create policy resource with properties
            PolicyTemplateResource policyTemplateResource = new PolicyTemplateResource()
            {
                name = $"[concat(parameters('ApimServiceName'), '/{apiName}/{policyPair.Key}/policy')]",
                type = ResourceTypeConstants.APIOperationPolicy,
                apiVersion = "2018-06-01-preview",
                properties = new PolicyTemplateProperties()
                {
                    contentFormat = isUrl ? "rawxml-link" : "rawxml",
                    policyContent = isUrl ? policyPair.Value.policy : this.fileReader.RetrieveLocalFileContents(policyPair.Value.policy)
                },
                dependsOn = dependsOn
            };
            return policyTemplateResource;
        }

        public List<PolicyTemplateResource> CreateOperationPolicyTemplateResources(CreatorConfig creatorConfig, string[] dependsOn)
        {
            // create a policy resource for each policy listed in the config file and its associated provided xml file
            List<PolicyTemplateResource> policyTemplateResources = new List<PolicyTemplateResource>();
            foreach (KeyValuePair<string, OperationsConfig> pair in creatorConfig.api.operations)
            {
                policyTemplateResources.Add(this.CreateOperationPolicyTemplateResource(pair, creatorConfig.api.name, dependsOn));
            }
            return policyTemplateResources;
        }
    }
}
