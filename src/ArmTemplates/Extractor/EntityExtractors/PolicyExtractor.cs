using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using System.IO;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class PolicyExtractor : TemplateGeneratorBase, IPolicyExtractor
    {
        public const string PoliciesDirectoryName = "policies";
        public const string GlobalServicePolicyFileName = "globalServicePolicy.xml";

        readonly IPolicyApiClient policyApiClient;

        public PolicyExtractor(IPolicyApiClient policyApiClient)
        {
            this.policyApiClient = policyApiClient;
        }

        public async Task<Template> GenerateGlobalServicePolicyTemplateAsync(ExtractorParameters extractorParameters, string baseFilesDirectory)
        {
            // extract global service policy in both full and single api extraction cases
            Template armTemplate = this.GenerateEmptyPropertyTemplateWithParameters();
            if (extractorParameters.PolicyXMLBaseUrl != null && extractorParameters.PolicyXMLSasToken != null)
            {
                TemplateParameterProperties policyTemplateSasTokenParameterProperties = new TemplateParameterProperties()
                {
                    type = "string"
                };
                armTemplate.Parameters.Add(ParameterNames.PolicyXMLSasToken, policyTemplateSasTokenParameterProperties);
            }

            if (extractorParameters.PolicyXMLBaseUrl != null)
            {
                TemplateParameterProperties policyTemplateBaseUrlParameterProperties = new TemplateParameterProperties()
                {
                    type = "string"
                };
                armTemplate.Parameters.Add(ParameterNames.PolicyXMLBaseUrl, policyTemplateBaseUrlParameterProperties);
            }

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // add global service policy resource to template
            try
            {
                var globalServicePolicyResource = await this.policyApiClient.GetGlobalServicePolicyAsync(
                    extractorParameters.SourceApimName, 
                    extractorParameters.ResourceGroup);

                Logger.LogInformation($"Global policy found for {extractorParameters.SourceApimName} API Management service");

                // REST API will return format property as rawxml and value property as the xml by default
                globalServicePolicyResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/policy')]";
                globalServicePolicyResource.ApiVersion = GlobalConstants.ApiVersion;
                globalServicePolicyResource.Scale = null;

                // write policy xml content to file and point to it if policyXMLBaseUrl is provided
                if (extractorParameters.PolicyXMLBaseUrl != null)
                {
                    await this.SavePolicyXmlAsync(globalServicePolicyResource, baseFilesDirectory);

                    globalServicePolicyResource.Properties.Format = "rawxml-link";
                    if (extractorParameters.PolicyXMLSasToken != null)
                    {
                        globalServicePolicyResource.Properties.Value = $"[concat(parameters('{ParameterNames.PolicyXMLBaseUrl}'), '{GlobalServicePolicyFileName}', parameters('{ParameterNames.PolicyXMLSasToken}'))]";
                    }
                    else
                    {
                        globalServicePolicyResource.Properties.Value = $"[concat(parameters('{ParameterNames.PolicyXMLBaseUrl}'), '{GlobalServicePolicyFileName}')]";
                    }
                }

                templateResources.Add(globalServicePolicyResource);
            }
            catch (Exception) { }

            armTemplate.Resources = templateResources.ToArray();
            return armTemplate;
        }

        async Task SavePolicyXmlAsync(PolicyTemplateResource policyTemplateResource, string baseFilesDirectory)
        {
            string policyXMLContent = policyTemplateResource.Properties.Value;
            var policiesDirectory = Path.Combine(baseFilesDirectory, PoliciesDirectoryName);

            // creating <files-root>/policies
            FileWriter.CreateFolderIfNotExists(policiesDirectory);

            // writing <files-root>/policies/globalServicePolicy.xml
            await FileWriter.SaveTextToFileAsync(policyXMLContent, policiesDirectory, GlobalServicePolicyFileName);
        }
    }
}
