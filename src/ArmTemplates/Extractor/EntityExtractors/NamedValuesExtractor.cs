// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class NamedValuesExtractor : INamedValuesExtractor
    {
        readonly ILogger<NamedValuesExtractor> logger;
        readonly ITemplateBuilder templateBuilder;
        
        readonly INamedValuesClient namedValuesClient;
        readonly IPolicyExtractor policyExtractor;
        readonly IBackendExtractor backendExtractor;

        public NamedValuesExtractor(
            ILogger<NamedValuesExtractor> logger,
            ITemplateBuilder templateBuilder,
            INamedValuesClient namedValuesClient,
            IPolicyExtractor policyExtractor,
            IBackendExtractor backendExtractor)
        {
            this.logger = logger;
            this.templateBuilder = templateBuilder;

            this.namedValuesClient = namedValuesClient;
            this.backendExtractor = backendExtractor;
            this.policyExtractor = policyExtractor;
        }

        public async Task<Template<NamedValuesResources>> GenerateNamedValuesTemplateAsync(
            string singleApiName,
            List<PolicyTemplateResource> apiPolicies,
            List<LoggerTemplateResource> loggerResources,
            ExtractorParameters extractorParameters,
            string baseFilesGenerationDirectory)
        {
            var namedValuesTemplate = this.templateBuilder
                                            .GenerateTemplateWithApimServiceNameProperty()
                                            .AddParameterizeNamedValueParameters(extractorParameters)
                                            .AddParameterizeNamedValuesKeyVaultSecretParameters(extractorParameters)
                                            .Build<NamedValuesResources>();
            
            if (extractorParameters.NotIncludeNamedValue)
            {
                this.logger.LogInformation("'{0}' parameter is turned on, so not processing named-values template generation at all.", nameof(extractorParameters.NotIncludeNamedValue));
                return namedValuesTemplate;
            }

            var namedValues = await this.namedValuesClient.GetAllAsync(extractorParameters);
            if (namedValues.IsNullOrEmpty())
            {
                this.logger.LogWarning("No named values found for apim '{0}'", extractorParameters.SourceApimName);
                return namedValuesTemplate;
            }
            
            foreach (var namedValueResource in namedValues)
            {
                // convert returned named value to template resource class
                namedValueResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{namedValueResource.OriginalName}')]";
                namedValueResource.Type = ResourceTypeConstants.NamedValues;
                namedValueResource.ApiVersion = GlobalConstants.ApiVersion;
                namedValueResource.Scale = null;

                this.ModifyNamedValuesOutput(namedValueResource, extractorParameters);

                if (string.IsNullOrEmpty(singleApiName))
                {
                    namedValuesTemplate.TypedResources.NamedValues.Add(namedValueResource);
                }
                else
                {
                    // if the user is executing a single api, extract all the named values used in the template resources
                    var foundInPolicy = this.DoesPolicyReferenceNamedValue(
                        apiPolicies,
                        namedValueResource.OriginalName, 
                        namedValueResource, 
                        baseFilesGenerationDirectory);

                    var foundInBackEnd = await this.backendExtractor.IsNamedValueUsedInBackends(
                        singleApiName,
                        apiPolicies,
                        namedValueResource.OriginalName,
                        namedValueResource.Properties.DisplayName,
                        extractorParameters,
                        baseFilesGenerationDirectory);

                    var foundInLogger = this.DoesLoggerReferenceNamedValue(
                        loggerResources,
                        namedValueResource.OriginalName, 
                        namedValueResource);

                    if (foundInPolicy || foundInBackEnd || foundInLogger)
                    {
                        namedValuesTemplate.TypedResources.NamedValues.Add(namedValueResource);
                    }
                }
            }

            return namedValuesTemplate;
        }

        bool DoesPolicyReferenceNamedValue(
            List<PolicyTemplateResource> apiPolicies, 
            string propertyName, 
            NamedValueTemplateResource namedValueResource,
            string baseFilesGenerationDirectory)
        {
            if (apiPolicies.IsNullOrEmpty())
            {
                return false;
            }

            foreach (var policyTemplateResource in apiPolicies)
            {
                var policyContent = this.policyExtractor.GetCachedPolicyContent(policyTemplateResource, baseFilesGenerationDirectory);

                if (policyContent.Contains(namedValueResource.Properties.DisplayName) || policyContent.Contains(propertyName))
                {
                    // dont need to go through all policies if the named value has already been found
                    return true;
                }
            }
            return false;
        }

        bool DoesLoggerReferenceNamedValue(
            List<LoggerTemplateResource> loggerTemplateResources, 
            string propertyName, 
            NamedValueTemplateResource propertyTemplateResource)
        {
            if (loggerTemplateResources.IsNullOrEmpty())
            {
                return false;
            }

            foreach (var logger in loggerTemplateResources)
            {
                if (logger.Properties.Credentials != null)
                {
                    if (!string.IsNullOrEmpty(logger.Properties.Credentials.ConnectionString) && logger.Properties.Credentials.ConnectionString.Contains(propertyName) ||
                        !string.IsNullOrEmpty(logger.Properties.Credentials.InstrumentationKey) && logger.Properties.Credentials.InstrumentationKey.Contains(propertyName) ||
                        !string.IsNullOrEmpty(logger.Properties.Credentials.ConnectionString) && logger.Properties.Credentials.ConnectionString.Contains(propertyTemplateResource.Properties.DisplayName) ||
                        !string.IsNullOrEmpty(logger.Properties.Credentials.InstrumentationKey) && logger.Properties.Credentials.InstrumentationKey.Contains(propertyTemplateResource.Properties.DisplayName))
                    {
                        // dont need to go through all loggers if the named value has already been found
                        return true;
                    }
                }
            }
            return false;
        }

        void ModifyNamedValuesOutput(NamedValueTemplateResource namedValueResource, ExtractorParameters extractorParameters)
        {
            if (extractorParameters.ParameterizeNamedValue)
            {
                namedValueResource.Properties.Value = $"[parameters('{ParameterNames.NamedValues}').{NamingHelper.GenerateValidParameterName(namedValueResource.OriginalName, ParameterPrefix.Property)}]";
            }

            //Hide the value field if it is a keyvault named value
            if (namedValueResource.Properties.KeyVault != null)
            {
                namedValueResource.Properties.Value = null;
            }

            if (namedValueResource.Properties.KeyVault != null && extractorParameters.ParamNamedValuesKeyVaultSecrets)
            {
                namedValueResource.Properties.KeyVault.SecretIdentifier = $"[parameters('{ParameterNames.NamedValueKeyVaultSecrets}').{NamingHelper.GenerateValidParameterName(namedValueResource.OriginalName, ParameterPrefix.Property)}]";
            }
        }
    }
}
