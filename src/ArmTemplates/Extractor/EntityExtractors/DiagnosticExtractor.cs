// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class DiagnosticExtractor : IDiagnosticExtractor
    {
        readonly ILogger<DiagnosticExtractor> logger;
        readonly IDiagnosticClient diagnosticClient;

        public DiagnosticExtractor(
            ILogger<DiagnosticExtractor> logger,
            IDiagnosticClient diagnosticClient)
        {
            this.logger = logger;
            this.diagnosticClient = diagnosticClient;
        }

        public async Task<List<DiagnosticTemplateResource>> GetApiDiagnosticsResourcesAsync(string apiName, ExtractorParameters extractorParameters)
        {
            var apiDiagnostics = await this.diagnosticClient.GetApiDiagnosticsAsync(apiName, extractorParameters);

            if (apiDiagnostics.IsNullOrEmpty())
            {
                this.logger.LogWarning("No diagnostics found for api {0}", apiName);
                return null;
            }

            var templateResources = new List<DiagnosticTemplateResource>();
            foreach (var apiDiagnostic in apiDiagnostics)
            {
                var apiDiagnosticOriginalName = apiDiagnostic.Name;

                apiDiagnostic.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{apiDiagnosticOriginalName}')]";
                apiDiagnostic.Type = ResourceTypeConstants.APIDiagnostic;
                apiDiagnostic.ApiVersion = GlobalConstants.ApiVersion;
                apiDiagnostic.Scale = null;
                apiDiagnostic.DependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiName}')]" };

                if (extractorParameters.ParameterizeApiLoggerId)
                {
                    apiDiagnostic.Properties.LoggerId = $"[parameters('{ParameterNames.ApiLoggerId}').{ParameterNamingHelper.GenerateValidParameterName(apiName, ParameterPrefix.Api)}.{ParameterNamingHelper.GenerateValidParameterName(apiDiagnosticOriginalName, ParameterPrefix.Diagnostic)}]";
                }

                if (!apiDiagnosticOriginalName.Contains("applicationinsights"))
                {
                    // enableHttpCorrelationHeaders only works for application insights, causes errors otherwise
                    apiDiagnostic.Properties.EnableHttpCorrelationHeaders = null;
                }

                templateResources.Add(apiDiagnostic);
            }

            return templateResources;
        }

        /// <summary>
        /// Gets the "All API" level diagnostic resources, these are common to all APIs.
        /// </summary>
        /// <param name="extractorParameters">The extractor.</param>
        /// <returns>a list of DiagnosticTemplateResources</returns>
        public async Task<List<DiagnosticTemplateResource>> GetServiceDiagnosticsTemplateResourcesAsync(ExtractorParameters extractorParameters)
        {
            var serviceDiagnostics = await this.diagnosticClient.GetAllAsync(extractorParameters);

            if (serviceDiagnostics.IsNullOrEmpty())
            {
                this.logger.LogWarning("No diagnostic settings were found...");
            }

            var templateResources = new List<DiagnosticTemplateResource>();
            foreach (var serviceDiagnostic in serviceDiagnostics)
            {
                this.logger.LogDebug("Found {0} diagnostic setting...", serviceDiagnostic.Name);
                var originalDiagnosticName = serviceDiagnostic.Name;

                serviceDiagnostic.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{originalDiagnosticName}')]";
                serviceDiagnostic.Type = ResourceTypeConstants.APIServiceDiagnostic;
                serviceDiagnostic.ApiVersion = GlobalConstants.ApiVersion;
                serviceDiagnostic.Scale = null;
                serviceDiagnostic.DependsOn = Array.Empty<string>();

                if (extractorParameters.ParameterizeApiLoggerId)
                {
                    serviceDiagnostic.Properties.LoggerId = $"[parameters('{ParameterNames.ApiLoggerId}').{ParameterNamingHelper.GenerateValidParameterName(originalDiagnosticName, ParameterPrefix.Diagnostic)}]";
                }

                if (!originalDiagnosticName.Contains("applicationinsights"))
                {
                    // enableHttpCorrelationHeaders only works for application insights, causes errors otherwise
                    //TODO: Check this settings still valid?
                    serviceDiagnostic.Properties.EnableHttpCorrelationHeaders = null;
                }

                templateResources.Add(serviceDiagnostic);
            }

            return templateResources;
        }
    }
}
