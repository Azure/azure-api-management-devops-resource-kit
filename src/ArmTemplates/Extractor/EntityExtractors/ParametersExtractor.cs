// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger.Cache;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class ParametersExtractor : IParametersExtractor
    {
        readonly ITemplateBuilder templateBuilder;
        readonly IApisClient apisClient;

        public ParametersExtractor(
            ITemplateBuilder templateBuilder,
            IApisClient apisClient)
        {
            this.templateBuilder = templateBuilder;
            this.apisClient = apisClient;
        }

        public async Task<Template> CreateMasterTemplateParameterValues(
            List<string> apisToExtract,
            LoggersCache loggersCache,
            LoggerTemplateResources loggerTemplateResources,
            BackendTemplateResources backendResources,
            NamedValuesResources namedValuesResources,
            ExtractorParameters extractorParameters)
        {
            // used to create the parameter values for use in parameters file
            // create empty template
            var parametersTemplate = this.templateBuilder
                .GenerateEmptyTemplate()
                .Build();
            parametersTemplate.Parameters = new();
            var parameters = parametersTemplate.Parameters;

            parameters.Add(ParameterNames.ApimServiceName, new() { Value = extractorParameters.DestinationApimName });
            AddLinkedUrlParameters();
            AddPolicyParameters();
            AddNamedValuesParameters();
            await AddServiceUrlParameterAsync();

            void AddLinkedUrlParameters()
            {
                if (string.IsNullOrEmpty(extractorParameters.LinkedTemplatesBaseUrl))
                {
                    return;
                }

                parameters.Add(ParameterNames.LinkedTemplatesBaseUrl, new() { Value = extractorParameters.LinkedTemplatesBaseUrl });

                if (!string.IsNullOrEmpty(extractorParameters.LinkedTemplatesSasToken))
                {
                    parameters.Add(ParameterNames.LinkedTemplatesSasToken, new() { Value = extractorParameters.LinkedTemplatesSasToken });
                }

                if (!string.IsNullOrEmpty(extractorParameters.LinkedTemplatesUrlQueryString))
                {
                    parameters.Add(ParameterNames.LinkedTemplatesUrlQueryString, new() { Value = extractorParameters.LinkedTemplatesUrlQueryString });
                }
            }

            void AddPolicyParameters()
            {
                if (string.IsNullOrEmpty(extractorParameters.PolicyXMLBaseUrl))
                {
                    return;
                }

                parameters.Add(ParameterNames.PolicyXMLBaseUrl, new() { Value = extractorParameters.PolicyXMLBaseUrl });

                if (!string.IsNullOrEmpty(extractorParameters.PolicyXMLSasToken))
                {
                    parameters.Add(ParameterNames.PolicyXMLSasToken, new() { Value = extractorParameters.PolicyXMLSasToken });
                }
            }

            async Task AddServiceUrlParameterAsync()
            {
                if (!extractorParameters.ParameterizeServiceUrl)
                {
                    return;
                }

                var serviceUrls = new Dictionary<string, string>();
                foreach (var apiName in apisToExtract)
                {
                    var validApiName = ParameterNamingHelper.GenerateValidParameterName(apiName, ParameterPrefix.Api);

                    string serviceUrl;
                    if (extractorParameters.ServiceUrlParameters is null)
                    {
                        var apiDetails = await this.apisClient.GetSingleAsync(apiName, extractorParameters);
                        serviceUrl = apiDetails.Properties.ServiceUrl;
                    }
                    else
                    {
                        serviceUrl = extractorParameters.ServiceUrlParameters.FirstOrDefault(x => x.ApiName.Equals(apiName))?.ServiceUrl;
                    }

                    serviceUrls.Add(validApiName, serviceUrl);
                }

                parameters.Add(ParameterNames.ServiceUrl, new TemplateObjectParameterProperties() { Value = serviceUrls });
            }

            void AddNamedValuesParameters()
            {
                if (!extractorParameters.ParameterizeNamedValue &&
                    !extractorParameters.ParamNamedValuesKeyVaultSecrets)
                {
                    return;
                }

                var namedValuesParameters = new Dictionary<string, string>();
                var keyVaultNamedValues = new Dictionary<string, string>();
                foreach (var namedValue in namedValuesResources.NamedValues)
                {
                    if (extractorParameters.ParameterizeNamedValue && namedValue?.Properties.KeyVault == null)
                    {
                        var propertyValue = namedValue.Properties.Value;
                        var validPName = ParameterNamingHelper.GenerateValidParameterName(namedValue.OriginalName, ParameterPrefix.Property);
                        namedValuesParameters.Add(validPName, propertyValue);
                    }

                    if (extractorParameters.ParamNamedValuesKeyVaultSecrets && namedValue?.Properties.KeyVault is not null)
                    {
                        var propertyValue = namedValue.Properties.KeyVault.SecretIdentifier;
                        var validPName = ParameterNamingHelper.GenerateValidParameterName(namedValue.OriginalName, ParameterPrefix.Property);
                        keyVaultNamedValues.Add(validPName, propertyValue);
                    }
                }

                parameters.Add(ParameterNames.NamedValues, new TemplateObjectParameterProperties() { Value = namedValuesParameters });
                parameters.Add(ParameterNames.NamedValueKeyVaultSecrets, new TemplateObjectParameterProperties() { Value = keyVaultNamedValues });
            }

            if (extractorParameters.ParameterizeApiLoggerId)
            {
                parameters.Add(ParameterNames.ApiLoggerId, new TemplateObjectParameterProperties() { Value = loggersCache.CreateResultingMap() });
            }

            if (extractorParameters.ParameterizeLogResourceId)
            {
                parameters.Add(ParameterNames.LoggerResourceId, new TemplateObjectParameterProperties() { Value = loggerTemplateResources.LoggerResourceIds });
            }

            if (extractorParameters.ParameterizeBackend)
            {
                parameters.Add(ParameterNames.BackendSettings, new TemplateObjectParameterProperties() { Value = backendResources.BackendNameParametersCache });
            }

            return parametersTemplate;
        }
    }
}
