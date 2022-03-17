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
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class PolicyExtractor : IPolicyExtractor
    {
        public const string PoliciesDirectoryName = "policies";
        public const string GlobalServicePolicyFileName = "globalServicePolicy.xml";

        public const string ProductPolicyFileNameFormat = "{0}-productPolicy.xml";
        public const string ApiPolicyFileNameFormat = "{0}-apiPolicy.xml";
        public const string ApiOperationPolicyFileNameFormat = "{0}-{1}-operationPolicy.xml";

        readonly ILogger<PolicyExtractor> logger;
        readonly IPolicyClient policyClient;
        readonly ITemplateBuilder templateBuilder;

        public PolicyExtractor(
            ILogger<PolicyExtractor> logger,
            IPolicyClient policyApiClient,
            ITemplateBuilder templateBuilder)
        {
            this.logger = logger;
            this.policyClient = policyApiClient;
            this.templateBuilder = templateBuilder;
        }

        public async Task<PolicyTemplateResource> GenerateApiOperationPolicyResourceAsync(
            string apiName,
            string operationName,
            string baseFilesGenerationDirectory,
            ExtractorParameters extractorParameters)
        {
            var apiOperationPolicy = await this.policyClient.GetPolicyLinkedToApiOperationAsync(apiName, operationName, extractorParameters);

            if (apiOperationPolicy is null)
            {
                this.logger.LogWarning("Policy for api '{0}' and operation '{1}' not found", apiName, operationName);
                return apiOperationPolicy;
            }

            var apiOperationPolicyOriginalName = apiOperationPolicy.Name;

            apiOperationPolicy.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{apiOperationPolicyOriginalName}/policy')]";
            apiOperationPolicy.ApiVersion = GlobalConstants.ApiVersion;
            apiOperationPolicy.Scale = null;
            apiOperationPolicy.DependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis/operations', parameters('{ParameterNames.ApimServiceName}'), '{apiName}', '{apiOperationPolicyOriginalName}')]" };

            // write policy xml content to file and point to it if policyXMLBaseUrl is provided
            if (extractorParameters.PolicyXMLBaseUrl is not null)
            {
                var policyFileName = string.Format(ApiOperationPolicyFileNameFormat, apiName, operationName);
                await this.SavePolicyXmlAsync(apiOperationPolicy, baseFilesGenerationDirectory, policyFileName);

                this.SetPolicyTemplateResourcePolicyContentWithArmPresetValues(extractorParameters, apiOperationPolicy, policyFileName);
            }

            return apiOperationPolicy;
        }

        public async Task<PolicyTemplateResource> GenerateApiPolicyResourceAsync(
            string apiName, 
            string baseFilesGenerationDirectory,
            ExtractorParameters extractorParameters)
        {
            var apiPolicy = await this.policyClient.GetPolicyLinkedToApiAsync(apiName, extractorParameters);

            if (apiPolicy is null)
            {
                this.logger.LogWarning("Policy for api {0} not found", apiName);
                return apiPolicy;
            }

            var apiPolicyOriginalName = apiPolicy.Name;

            apiPolicy.ApiVersion = GlobalConstants.ApiVersion;
            apiPolicy.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{apiPolicyOriginalName}')]";
            apiPolicy.DependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiName}')]" };

            // write policy xml content to file and point to it if policyXMLBaseUrl is provided
            if (extractorParameters.PolicyXMLBaseUrl is not null)
            {
                var policyFileName = string.Format(ApiPolicyFileNameFormat, apiName);
                await this.SavePolicyXmlAsync(apiPolicy, baseFilesGenerationDirectory, policyFileName);

                this.SetPolicyTemplateResourcePolicyContentWithArmPresetValues(extractorParameters, apiPolicy, policyFileName);
            }

            return apiPolicy;
        }

        public async Task<PolicyTemplateResource> GenerateProductPolicyTemplateAsync(
            ExtractorParameters extractorParameters,
            string productName,
            string[] productResourceId,
            string baseFilesGenerationDirectory)
        {
            var productPolicy = await this.policyClient.GetPolicyLinkedToProductAsync(productName, extractorParameters);

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
            if (!string.IsNullOrWhiteSpace(extractorParameters.PolicyXMLBaseUrl))
            {
                var policyFileName = string.Format(ProductPolicyFileNameFormat, productName);
                await this.SavePolicyXmlAsync(productPolicy, baseFilesGenerationDirectory, policyFileName);

                this.SetPolicyTemplateResourcePolicyContentWithArmPresetValues(extractorParameters, productPolicy, policyFileName);
            }

            return productPolicy;
        }

        public async Task<Template<PolicyTemplateResources>> GenerateGlobalServicePolicyTemplateAsync(ExtractorParameters extractorParameters, string baseFilesDirectory)
        {
            // extract global service policy in both full and single api extraction cases
            var policyTemplate = this.templateBuilder.GenerateTemplateWithApimServiceNameProperty()
                                                       .AddPolicyProperties(extractorParameters)
                                                       .Build<PolicyTemplateResources>();

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
                if (!string.IsNullOrWhiteSpace(extractorParameters.PolicyXMLBaseUrl))
                {
                    // writing to globalServicePolicy.xml (<files-root>/policies/globalServicePolicy.xml)
                    await this.SavePolicyXmlAsync(globalServicePolicyResource, baseFilesDirectory, GlobalServicePolicyFileName);

                    this.SetPolicyTemplateResourcePolicyContentWithArmPresetValues(extractorParameters, globalServicePolicyResource, GlobalServicePolicyFileName);
                }

                policyTemplate.TypedResources.GlobalServicePolicy = globalServicePolicyResource;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Exception occured while global apim service policy generation");
            }

            return policyTemplate;
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
