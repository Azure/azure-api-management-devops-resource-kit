using System;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class PolicyExtractor: EntityExtractor
    {
        private FileWriter fileWriter;

        public PolicyExtractor(FileWriter fileWriter)
        {
            this.fileWriter = fileWriter;
        }

        public async Task<string> GetGlobalServicePolicyAsync(string ApiManagementName, string ResourceGroupName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/policies/policy?api-version={4}&format=rawxml",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<Template> GenerateGlobalServicePolicyTemplateAsync(string apimname, string resourceGroup, string policyXMLBaseUrl, string policyXMLSasToken, string fileFolder)
        {
            // extract global service policy in both full and single api extraction cases
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting global service policy from service");
            Template armTemplate = GenerateEmptyTemplateWithParameters(policyXMLBaseUrl, policyXMLSasToken);

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // add global service policy resource to template
            try
            {
                string globalServicePolicy = await GetGlobalServicePolicyAsync(apimname, resourceGroup);
                Console.WriteLine($" - Global policy found for {apimname} API Management service");
                PolicyTemplateResource globalServicePolicyResource = JsonConvert.DeserializeObject<PolicyTemplateResource>(globalServicePolicy);
                // REST API will return format property as rawxml and value property as the xml by default
                globalServicePolicyResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/policy')]";
                globalServicePolicyResource.apiVersion = GlobalConstants.APIVersion;
                globalServicePolicyResource.scale = null;

                // write policy xml content to file and point to it if policyXMLBaseUrl is provided
                if (policyXMLBaseUrl != null)
                {
                    string policyXMLContent = globalServicePolicyResource.properties.value;
                    string policyFolder = String.Concat(fileFolder, $@"/policies");
                    string globalServicePolicyFileName = $@"/globalServicePolicy.xml";
                    this.fileWriter.CreateFolderIfNotExists(policyFolder);
                    this.fileWriter.WriteXMLToFile(policyXMLContent, String.Concat(policyFolder, globalServicePolicyFileName));
                    globalServicePolicyResource.properties.format = "rawxml-link";
                    if (policyXMLSasToken != null)
                    {
                        globalServicePolicyResource.properties.value = $"[concat(parameters('{ParameterNames.PolicyXMLBaseUrl}'), '{globalServicePolicyFileName}', parameters('{ParameterNames.PolicyXMLSasToken}'))]";
                    }
                    else
                    {
                        globalServicePolicyResource.properties.value = $"[concat(parameters('{ParameterNames.PolicyXMLBaseUrl}'), '{globalServicePolicyFileName}')]";
                    }
                }

                templateResources.Add(globalServicePolicyResource);
            }
            catch (Exception) { }

            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}
