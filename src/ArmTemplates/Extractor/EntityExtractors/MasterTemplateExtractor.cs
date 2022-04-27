// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiVersionSet;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.TagApi;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.AuthorizationServer;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Master;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class MasterTemplateExtractor : IMasterTemplateExtractor
    {
        readonly ILogger<MasterTemplateExtractor> logger;
        readonly ITemplateBuilder templateBuilder;

        public MasterTemplateExtractor(
            ILogger<MasterTemplateExtractor> logger,
            ITemplateBuilder templateBuilder)
        {
            this.logger = logger;
            this.templateBuilder = templateBuilder;
        }

        public Template<MasterTemplateResources> GenerateLinkedMasterTemplate(
            ExtractorParameters extractorParameters,
            ApiTemplateResources apiTemplateResources = null,
            PolicyTemplateResources policyTemplateResources = null,
            ApiVersionSetTemplateResources apiVersionSetTemplateResources = null,
            ProductTemplateResources productsTemplateResources = null,
            ProductApiTemplateResources productAPIsTemplateResources = null,
            TagApiTemplateResources apiTagsTemplateResources = null,
            LoggerTemplateResources loggersTemplateResources = null,
            BackendTemplateResources backendsTemplateResources = null,
            AuthorizationServerTemplateResources authorizationServersTemplateResources = null,
            NamedValuesResources namedValuesTemplateResources = null,
            TagTemplateResources tagTemplateResources = null)
        {
            var masterTemplate = this.templateBuilder
                                        .GenerateEmptyTemplate()
                                        .Build<MasterTemplateResources>();
            masterTemplate.Parameters = this.CreateMasterTemplateParameters(extractorParameters);
            
            var masterResources = masterTemplate.TypedResources;
            var fileNames = extractorParameters.FileNames;

            // all other deployment resources will depend on named values
            var dependsOnNamedValues = Array.Empty<string>();

            // api dependsOn
            var apiDependsOn = new List<string>();
            var productApiDependsOn = new List<string>();
            var apiTagDependsOn = new List<string>();

            if (namedValuesTemplateResources?.HasContent() == true)
            {
                this.logger.LogDebug("Adding named-values to master template");
                const string NamedValuesTemplateName = "namedValuesTemplate";

                dependsOnNamedValues = new string[] { $"[resourceId('{ResourceTypeConstants.ArmDeployments}', '{NamedValuesTemplateName}')]" };
                apiDependsOn.Add($"[resourceId('{ResourceTypeConstants.ArmDeployments}', '{NamedValuesTemplateName}')]");
                var namedValuesUri = this.GenerateLinkedTemplateUri(fileNames.NamedValues, extractorParameters);
                var namedValuesDeployment = CreateLinkedMasterTemplateResourceForPropertyTemplate(
                    NamedValuesTemplateName,
                    namedValuesUri,
                    Array.Empty<string>(),
                    extractorParameters);

                masterResources.DeploymentResources.Add(namedValuesDeployment);
            }

            if (policyTemplateResources?.HasContent() == true)
            {
                this.logger.LogDebug("Adding policy to master template");
                const string GlobalServicePolicyTemplate = "globalServicePolicyTemplate";

                apiDependsOn.Add($"[resourceId('{ResourceTypeConstants.ArmDeployments}', '{GlobalServicePolicyTemplate}')]");
                var globalServicePolicyUri = this.GenerateLinkedTemplateUri(fileNames.GlobalServicePolicy, extractorParameters);
                var policyDeployment = CreateLinkedMasterTemplateResourceWithPolicyToken(
                    GlobalServicePolicyTemplate,
                    globalServicePolicyUri,
                    dependsOnNamedValues,
                    extractorParameters);

                masterResources.DeploymentResources.Add(policyDeployment);
            }

            if (apiVersionSetTemplateResources?.HasContent() == true)
            {
                this.logger.LogDebug("Adding api-version-set to master template");
                const string VersionSetTemplate = "versionSetTemplate";

                apiDependsOn.Add($"[resourceId('{ResourceTypeConstants.ArmDeployments}', '{VersionSetTemplate}')]");
                string apiVersionSetUri = this.GenerateLinkedTemplateUri(fileNames.ApiVersionSets, extractorParameters);
                var apiVersionSetDeployment = CreateLinkedMasterTemplateResource(VersionSetTemplate, apiVersionSetUri, dependsOnNamedValues);

                masterResources.DeploymentResources.Add(apiVersionSetDeployment);
            }

            if (productsTemplateResources?.HasContent() == true)
            {
                this.logger.LogDebug("Adding products to master template");
                const string ProductsTemplate = "productsTemplate";

                apiDependsOn.Add($"[resourceId('{ResourceTypeConstants.ArmDeployments}', '{ProductsTemplate}')]");
                productApiDependsOn.Add($"[resourceId('{ResourceTypeConstants.ArmDeployments}', '{ProductsTemplate}')]");
                var productsUri = this.GenerateLinkedTemplateUri(fileNames.Products, extractorParameters);

                var productDeployment = CreateLinkedMasterTemplateResource(ProductsTemplate, productsUri, dependsOnNamedValues);
                
                if (!string.IsNullOrEmpty(extractorParameters.PolicyXMLBaseUrl))
                {
                    productDeployment.Properties.Parameters.Add(ParameterNames.PolicyXMLBaseUrl, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.PolicyXMLBaseUrl}')]" });
                }

                if (!string.IsNullOrEmpty(extractorParameters.PolicyXMLSasToken))
                {
                    productDeployment.Properties.Parameters.Add(ParameterNames.PolicyXMLSasToken, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.PolicyXMLSasToken}')]" });
                }

                masterResources.DeploymentResources.Add(productDeployment);
            }

            if (tagTemplateResources?.HasContent() == true)
            {
                this.logger.LogDebug("Adding tags to master template");
                const string TagTemplate = "tagTemplate";

                apiTagDependsOn.Add($"[resourceId('{ResourceTypeConstants.ArmDeployments}', '{TagTemplate}')]");
                apiDependsOn.Add($"[resourceId('{ResourceTypeConstants.ArmDeployments}', '{TagTemplate}')]");

                var tagUri = this.GenerateLinkedTemplateUri(fileNames.Tags, extractorParameters);
                var tagDeployment = CreateLinkedMasterTemplateResource(TagTemplate, tagUri, dependsOnNamedValues);
                
                masterResources.DeploymentResources.Add(tagDeployment);
            }

            if (loggersTemplateResources?.HasContent() == true)
            {
                this.logger.LogDebug("Adding loggers to master template");
                const string LoggersTemplate = "loggersTemplate";

                apiDependsOn.Add($"[resourceId('{ResourceTypeConstants.ArmDeployments}', '{LoggersTemplate}')]");
                string loggersUri = this.GenerateLinkedTemplateUri(fileNames.Loggers, extractorParameters);
                var loggersDeployment = CreateLinkedMasterTemplateResourceForLoggerTemplate(LoggersTemplate, loggersUri, dependsOnNamedValues, extractorParameters);

                masterResources.DeploymentResources.Add(loggersDeployment);
            }

            if (backendsTemplateResources?.HasContent() == true)
            {
                this.logger.LogDebug("Adding backends to master template");
                const string BackendsTemplate = "backendsTemplate";

                apiDependsOn.Add($"[resourceId('{ResourceTypeConstants.ArmDeployments}', '{BackendsTemplate}')]");
                string backendsUri = this.GenerateLinkedTemplateUri(fileNames.Backends, extractorParameters);
                var backendsDeployment = CreateLinkedMasterTemplateResource(BackendsTemplate, backendsUri, dependsOnNamedValues);
                
                if (extractorParameters.ParameterizeBackend)
                {
                    backendsDeployment.Properties.Parameters.Add(ParameterNames.BackendSettings,
                        new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.BackendSettings}')]" });
                }

                masterResources.DeploymentResources.Add(backendsDeployment);
            }

            if (authorizationServersTemplateResources?.HasContent() == true)
            {
                this.logger.LogDebug("Adding authorizationServers to master template");
                const string AuthorizationServersTemplate = "authorizationServersTemplate";

                apiDependsOn.Add($"[resourceId('{ResourceTypeConstants.ArmDeployments}', '{AuthorizationServersTemplate}')]");
                var authorizationServersUri = this.GenerateLinkedTemplateUri(fileNames.AuthorizationServers, extractorParameters);
                var authorizationServersDeployment = CreateLinkedMasterTemplateResource(AuthorizationServersTemplate, authorizationServersUri, dependsOnNamedValues);

                masterResources.DeploymentResources.Add(authorizationServersDeployment);
            }

            if (apiTemplateResources?.HasContent() == true)
            {
                this.logger.LogDebug("Adding apis to master template");
                const string ApisTemplate = "apisTemplate";

                apiTagDependsOn.Add($"[resourceId('{ResourceTypeConstants.ArmDeployments}', '{ApisTemplate}')]");
                productApiDependsOn.Add($"[resourceId('{ResourceTypeConstants.ArmDeployments}', '{ApisTemplate}')]");
                var apisUri = this.GenerateLinkedTemplateUri(apiTemplateResources.FileName, extractorParameters);
                var apisDeployment = CreateLinkedMasterTemplateResourceForApiTemplate(ApisTemplate, apisUri, apiDependsOn.ToArray(), extractorParameters);

                masterResources.DeploymentResources.Add(apisDeployment);
            }

            if (productAPIsTemplateResources?.HasContent() == true)
            {
                this.logger.LogDebug("Adding productApis to master template");
                const string ProductApisTemplate = "productAPIsTemplate";

                var productApisUri = this.GenerateLinkedTemplateUri(fileNames.ProductAPIs, extractorParameters);
                var productApisDeployment = CreateLinkedMasterTemplateResource(ProductApisTemplate, productApisUri, productApiDependsOn.ToArray());

                masterResources.DeploymentResources.Add(productApisDeployment);
            }

            if (apiTagsTemplateResources?.HasContent() == true)
            {
                this.logger.LogDebug("Adding apiTags to master template");
                const string ApiTagsTemplate = "apiTagsTemplate";

                var apiTagsUri = this.GenerateLinkedTemplateUri(fileNames.TagApi, extractorParameters);
                var apiTagsDeployment = CreateLinkedMasterTemplateResource(ApiTagsTemplate, apiTagsUri, apiTagDependsOn.ToArray());

                masterResources.DeploymentResources.Add(apiTagsDeployment);
            }
            
            return masterTemplate;
        }

        static MasterTemplateResource CreateLinkedMasterTemplateResourceForApiTemplate(string name, string uriLink, string[] dependsOn, ExtractorParameters extractorParameters)
        {
            MasterTemplateResource masterResourceTemplate = CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);
            if (extractorParameters.PolicyXMLBaseUrl != null)
            {
                masterResourceTemplate.Properties.Parameters.Add(ParameterNames.PolicyXMLBaseUrl, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.PolicyXMLBaseUrl}')]" });
            }
            if (extractorParameters.PolicyXMLSasToken != null)
            {
                masterResourceTemplate.Properties.Parameters.Add(ParameterNames.PolicyXMLSasToken, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.PolicyXMLSasToken}')]" });
            }
            if (extractorParameters.ParameterizeServiceUrl)
            {
                masterResourceTemplate.Properties.Parameters.Add(ParameterNames.ServiceUrl, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.ServiceUrl}')]" });
            }
            if (extractorParameters.ParameterizeApiLoggerId)
            {
                masterResourceTemplate.Properties.Parameters.Add(ParameterNames.ApiLoggerId, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.ApiLoggerId}')]" });
            }
            return masterResourceTemplate;
        }

        static MasterTemplateResource CreateLinkedMasterTemplateResourceForPropertyTemplate(string name, string uriLink, string[] dependsOn, ExtractorParameters extractorParameters)
        {
            var masterResourceTemplate = CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);
            if (extractorParameters.ParameterizeNamedValue)
            {
                masterResourceTemplate.Properties.Parameters.Add(ParameterNames.NamedValues, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.NamedValues}')]" });
            }
            if (extractorParameters.ParamNamedValuesKeyVaultSecrets)
            {
                masterResourceTemplate.Properties.Parameters.Add(ParameterNames.NamedValueKeyVaultSecrets, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.NamedValueKeyVaultSecrets}')]" });
            }
            return masterResourceTemplate;
        }

        static MasterTemplateResource CreateLinkedMasterTemplateResourceWithPolicyToken(string name, string uriLink, string[] dependsOn, ExtractorParameters extractorParameters)
        {
            MasterTemplateResource masterResourceTemplate = CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);

            if (extractorParameters.PolicyXMLBaseUrl != null)
            {
                masterResourceTemplate.Properties.Parameters.Add(ParameterNames.PolicyXMLBaseUrl, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.PolicyXMLBaseUrl}')]" });
            }
            if (extractorParameters.PolicyXMLSasToken != null)
            {
                masterResourceTemplate.Properties.Parameters.Add(ParameterNames.PolicyXMLSasToken, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.PolicyXMLSasToken}')]" });
            }
            return masterResourceTemplate;
        }

        static MasterTemplateResource CreateLinkedMasterTemplateResourceForLoggerTemplate(string name, string uriLink, string[] dependsOn, ExtractorParameters extractorParameters)
        {
            MasterTemplateResource masterResourceTemplate = CreateLinkedMasterTemplateResource(name, uriLink, dependsOn);
            if (extractorParameters.ParameterizeLogResourceId)
            {
                masterResourceTemplate.Properties.Parameters.Add(ParameterNames.LoggerResourceId, new TemplateParameterProperties() { Value = $"[parameters('{ParameterNames.LoggerResourceId}')]" });
            }
            return masterResourceTemplate;
        }

        static MasterTemplateResource CreateLinkedMasterTemplateResource(string name, string uriLink, string[] dependsOn)
        {
            // create deployment resource with provided arguments
            MasterTemplateResource masterTemplateResource = new MasterTemplateResource()
            {
                Name = name,
                Type = ResourceTypeConstants.ArmDeployments,
                ApiVersion = GlobalConstants.ArmApiVersion,
                Properties = new MasterTemplateProperties()
                {
                    Mode = "Incremental",
                    TemplateLink = new MasterTemplateLink()
                    {
                        Uri = uriLink,
                        ContentVersion = "1.0.0.0"
                    },
                    Parameters = new Dictionary<string, TemplateParameterProperties>
                    {
                        { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ Value = $"[parameters('{ParameterNames.ApimServiceName}')]" } }
                    }
                },
                DependsOn = dependsOn
            };
            return masterTemplateResource;
        }

        Dictionary<string, TemplateParameterProperties> CreateMasterTemplateParameters(ExtractorParameters extractorParameters)
        {
            // used to create the parameter metatadata, etc (not value) for use in file with resources
            // add parameters with metatdata properties
            var parameters = new Dictionary<string, TemplateParameterProperties>();

            parameters.Add(
                ParameterNames.ApimServiceName, 
                new TemplateParameterProperties(metadataDescription: "Name of the API Management", type: "string"));
            
            // add remote location of template files for linked option
            parameters.Add(
                ParameterNames.LinkedTemplatesBaseUrl,
                new TemplateParameterProperties(metadataDescription: "Base URL of the repository that contains the generated templates", type: "string"));

            // add linkedTemplatesSasToken parameter if provided and if the templates are linked
            if (!string.IsNullOrEmpty(extractorParameters.LinkedTemplatesSasToken))
            {
                parameters.Add(
                    ParameterNames.LinkedTemplatesSasToken,
                    new TemplateParameterProperties(metadataDescription: "The Shared Access Signature for the URL of the repository", type: "string"));
            }
            
            // add linkedTemplatesUrlQueryString parameter if provided and if the templates are linked
            if (!string.IsNullOrEmpty(extractorParameters.LinkedTemplatesUrlQueryString))
            {
                parameters.Add(
                    ParameterNames.LinkedTemplatesUrlQueryString,
                    new TemplateParameterProperties(metadataDescription: "Query string for the URL of the repository", type: "string"));
            }

            if (!string.IsNullOrEmpty(extractorParameters.PolicyXMLBaseUrl))
            {
                parameters.Add(
                    ParameterNames.PolicyXMLBaseUrl,
                    new TemplateParameterProperties(metadataDescription: "Base URL of the repository that contains the generated policy files", type: "string"));

                if (!string.IsNullOrEmpty(extractorParameters.PolicyXMLSasToken))
                {
                    parameters.Add(
                        ParameterNames.PolicyXMLSasToken,
                        new TemplateParameterProperties(metadataDescription: "The SAS token for the URL of the policy container", type: "string"));
                }
            }

            if (extractorParameters.ParameterizeServiceUrl)
            {
                parameters.Add(
                    ParameterNames.ServiceUrl,
                    new TemplateParameterProperties(metadataDescription: "Service url for each Api", type: "object"));
            }

            if (extractorParameters.ParameterizeNamedValue)
            {
                parameters.Add(
                    ParameterNames.NamedValues,
                    new TemplateParameterProperties(metadataDescription: "Named values", type: "object"));
            }

            if (extractorParameters.ParameterizeApiLoggerId)
            {
                parameters.Add(
                    ParameterNames.ApiLoggerId,
                    new TemplateParameterProperties(metadataDescription: "LoggerId for this api", type: "object"));
            }

            if (extractorParameters.ParameterizeLogResourceId)
            {
                parameters.Add(
                    ParameterNames.LoggerResourceId,
                    new TemplateParameterProperties(metadataDescription: "ResourceId for the logger", type: "object"));
            }

            if (extractorParameters.ParamNamedValuesKeyVaultSecrets)
            {
                parameters.Add(
                    ParameterNames.NamedValueKeyVaultSecrets,
                    new TemplateParameterProperties(metadataDescription: "Key Vault Secrets for Named Values", type: "object"));
            }

            if (extractorParameters.ParameterizeBackend)
            {
                parameters.Add(
                    ParameterNames.BackendSettings,
                    new TemplateParameterProperties(metadataDescription: "The settings for the Backends", type: "object"));
            }

            return parameters;
        }

        string GenerateLinkedTemplateUri(string fileName, ExtractorParameters extractorParameters)
        {
            var linkedTemplateUri = !string.IsNullOrEmpty(extractorParameters.LinkedTemplatesSasToken)
                ? $"parameters('{ParameterNames.LinkedTemplatesBaseUrl}'), '{fileName}', parameters('{ParameterNames.LinkedTemplatesSasToken}')"
                : $"parameters('{ParameterNames.LinkedTemplatesBaseUrl}'), '{fileName}'";

            return !string.IsNullOrEmpty(extractorParameters.LinkedTemplatesUrlQueryString)
                ? $"[concat({linkedTemplateUri}, parameters('{ParameterNames.LinkedTemplatesUrlQueryString}'))]" 
                : $"[concat({linkedTemplateUri})]";
        }
    }
}
