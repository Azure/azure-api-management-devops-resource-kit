using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class PolicyExtractor : EntityExtractorBase, IPolicyExtractor
    {
        public async Task<string> GetGlobalServicePolicyAsync(string ApiManagementName, string ResourceGroupName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/policies/policy?api-version={4}&format=rawxml",
               BaseUrl, azSubId, ResourceGroupName, ApiManagementName, GlobalConstants.APIVersion);

            return await this.CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<Template> GenerateGlobalServicePolicyTemplateAsync(string apimname, string resourceGroup, string policyXMLBaseUrl, string policyXMLSasToken, string fileFolder)
        {
            // extract global service policy in both full and single api extraction cases
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting global service policy from service");
            Template armTemplate = this.GenerateEmptyPropertyTemplateWithParameters();
            if (policyXMLBaseUrl != null && policyXMLSasToken != null)
            {
                TemplateParameterProperties policyTemplateSasTokenParameterProperties = new TemplateParameterProperties()
                {
                    type = "string"
                };
                armTemplate.parameters.Add(ParameterNames.PolicyXMLSasToken, policyTemplateSasTokenParameterProperties);
            }
            if (policyXMLBaseUrl != null)
            {
                TemplateParameterProperties policyTemplateBaseUrlParameterProperties = new TemplateParameterProperties()
                {
                    type = "string"
                };
                armTemplate.parameters.Add(ParameterNames.PolicyXMLBaseUrl, policyTemplateBaseUrlParameterProperties);
            }

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // add global service policy resource to template
            try
            {
                string globalServicePolicy = await this.GetGlobalServicePolicyAsync(apimname, resourceGroup);
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
                    string policyFolder = string.Concat(fileFolder, $@"/policies");
                    string globalServicePolicyFileName = $@"/globalServicePolicy.xml";
                    FileWriter.CreateFolderIfNotExists(policyFolder);
                    FileWriter.WriteXMLToFile(policyXMLContent, string.Concat(policyFolder, globalServicePolicyFileName));
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
