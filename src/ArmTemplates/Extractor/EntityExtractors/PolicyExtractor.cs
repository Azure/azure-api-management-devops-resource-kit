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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class PolicyExtractor : TemplateGeneratorBase, IPolicyExtractor
    {
        public const string PoliciesDirectoryName = "policies";
        public const string GlobalServicePolicyFileName = "globalServicePolicy.xml";

        public static string ProductPolicyFileNameFormat = "{0}-productPolicy.xml";

        readonly ILogger<PolicyExtractor> logger;
        readonly IPolicyClient policyClient;

        public PolicyExtractor(
            ILogger<PolicyExtractor> logger,
            IPolicyClient policyApiClient)
        {
            this.logger = logger;
            this.policyClient = policyApiClient;
        }

        public async Task<PolicyTemplateResource> GenerateProductPolicyTemplateAsync(
            ExtractorParameters extractorParameters, 
            string productName,
            string[] productResourceId,
            string baseFilesGenerationDirectory)
        {
            var productPolicy = await this.policyClient.GetPolicyLinkedToProductAsync(extractorParameters, productName);

            if (productPolicy is null)
            {
                this.logger.LogWarning($"Policy not found for product '{productName}'");
                return productPolicy;
            }

            this.logger.LogDebug($"Policy linked to {productName} product found successfuly");

            productPolicy.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{productName}/policy')]";
            productPolicy.ApiVersion = GlobalConstants.ApiVersion;
            productPolicy.Scale = null;
            productPolicy.DependsOn = productResourceId;

            // write policy xml content to file and point to it if policyXMLBaseUrl is provided
            if (!string.IsNullOrEmpty(extractorParameters.PolicyXMLBaseUrl))
            {
                var policyFileName = string.Format(ProductPolicyFileNameFormat, productName);
                await this.SavePolicyXmlAsync(productPolicy, baseFilesGenerationDirectory, policyFileName);

                this.SetPolicyTemplateResourcePolicyContentWithArmPresetValues(extractorParameters, productPolicy, policyFileName);
            }

            return productPolicy;
        }

        public async Task<Template> GenerateGlobalServicePolicyTemplateAsync(ExtractorParameters extractorParameters, string baseFilesDirectory)
        {
            // extract global service policy in both full and single api extraction cases
            Template armTemplate = this.GenerateTemplateWithApimServiceNameProperty()
                                       .AddPolicyProperties(extractorParameters);

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // add global service policy resource to template
            try
            {
                var globalServicePolicyResource = await this.policyClient.GetGlobalServicePolicyAsync(extractorParameters);

                Logger.LogInformation($"Global policy found for {extractorParameters.SourceApimName} API Management service");

                // REST API will return format property as rawxml and value property as the xml by default
                globalServicePolicyResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/policy')]";
                globalServicePolicyResource.ApiVersion = GlobalConstants.ApiVersion;
                globalServicePolicyResource.Scale = null;

                // write policy xml content to file and point to it if policyXMLBaseUrl is provided
                if (!string.IsNullOrEmpty(extractorParameters.PolicyXMLBaseUrl))
                {
                    // writing to globalServicePolicy.xml (<files-root>/policies/globalServicePolicy.xml)
                    await this.SavePolicyXmlAsync(globalServicePolicyResource, baseFilesDirectory, GlobalServicePolicyFileName);

                    this.SetPolicyTemplateResourcePolicyContentWithArmPresetValues(extractorParameters, globalServicePolicyResource, GlobalServicePolicyFileName);
                }

                templateResources.Add(globalServicePolicyResource);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Exception occured while global apim service policy generation");
            }

            armTemplate.Resources = templateResources.ToArray();
            return armTemplate;
        }

        void SetPolicyTemplateResourcePolicyContentWithArmPresetValues(
            ExtractorParameters extractorParameters, 
            PolicyTemplateResource policyTemplate,
            string policyFileName)
        {
            policyTemplate.Properties.Format = "rawxml-link";
            if (extractorParameters.PolicyXMLSasToken != null)
            {
                policyTemplate.Properties.PolicyContent = $"[concat(parameters('{ParameterNames.PolicyXMLBaseUrl}'), '{policyFileName}', parameters('{ParameterNames.PolicyXMLSasToken}'))]";
            }
            else
            {
                policyTemplate.Properties.PolicyContent = $"[concat(parameters('{ParameterNames.PolicyXMLBaseUrl}'), '{policyFileName}')]";
            }
        }

        async Task SavePolicyXmlAsync(
            PolicyTemplateResource policyTemplateResource, 
            string baseFilesDirectory,
            string policyFileName)
        {
            string policyXMLContent = policyTemplateResource.Properties.PolicyContent;
            var policiesDirectory = Path.Combine(baseFilesDirectory, PoliciesDirectoryName);

            // creating <files-root>/policies
            FileWriter.CreateFolderIfNotExists(policiesDirectory);

            // writing <files-root>/policies/<policyFileName>.xml
            await FileWriter.SaveTextToFileAsync(policyXMLContent, policiesDirectory, policyFileName);
        }
    }
}
