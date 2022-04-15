// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger.Cache;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class LoggerExtractor : ILoggerExtractor
    {
        readonly ILogger<LoggerExtractor> logger;
        readonly IDiagnosticClient diagnosticClient;
        readonly ILoggerClient loggerClient;
        readonly ITemplateBuilder templateBuilder;

        public LoggersCache Cache { get; private set; } = new LoggersCache();

        public LoggerExtractor(
            ILogger<LoggerExtractor> logger,
            ITemplateBuilder templateBuilder,
            ILoggerClient loggerClient,
            IDiagnosticClient diagnosticClient)
        {
            this.logger = logger;
            this.templateBuilder = templateBuilder;
            this.diagnosticClient = diagnosticClient;
            this.loggerClient = loggerClient;
        }        

        public async Task<Template<LoggerTemplateResources>> GenerateLoggerTemplateAsync(
            List<string> apisToExtract,
            List<PolicyTemplateResource> apiPolicies,
            ExtractorParameters extractorParameters)
        {
            var loggerTemplate = this.templateBuilder
                                        .GenerateTemplateWithApimServiceNameProperty()
                                        .Build<LoggerTemplateResources>();

            if (extractorParameters.ParameterizeApiLoggerId)
            {
                await this.LoadAllReferencedLoggers(apisToExtract, extractorParameters);
            }

            var loggers = await this.loggerClient.GetAllAsync(extractorParameters);
            foreach (var logger in loggers)
            {
                var originalLoggerName = logger.Name;

                logger.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{originalLoggerName}')]";
                logger.Type = ResourceTypeConstants.Logger;
                logger.ApiVersion = GlobalConstants.ApiVersion;
                logger.Scale = null;

                // single api extraction
                if (apisToExtract?.Count != 1)
                {
                    loggerTemplate.TypedResources.Loggers.Add(logger);
                }
                else
                {
                    // if the user is extracting a single api, extract the loggers referenced by its diagnostics and api policies
                    var isReferencedInPolicy = apiPolicies?.Any(x => x.Properties.PolicyContent.Contains(originalLoggerName));
                    
                    bool isReferencedInDiagnostic = false;
                    var validApiName = ParameterNamingHelper.GenerateValidParameterName(apisToExtract.First(), ParameterPrefix.Api);
                    if (extractorParameters.ParameterizeApiLoggerId && this.Cache.ApiDiagnosticLoggerBindings.ContainsKey(validApiName))
                    {
                        var diagnosticLoggerBindings = this.Cache.ApiDiagnosticLoggerBindings[validApiName];
                        
                        if (!diagnosticLoggerBindings.IsNullOrEmpty())
                        {
                            var validDiagnosticName = ParameterNamingHelper.GenerateValidParameterName(logger.Properties.LoggerType, ParameterPrefix.Diagnostic).ToLower();
                            if (diagnosticLoggerBindings.Any(x => x.DiagnosticName == validDiagnosticName))
                            {
                                isReferencedInDiagnostic = true;
                            }
                        }
                    }

                    if (isReferencedInPolicy == true || isReferencedInDiagnostic)
                    {
                        // logger was used in policy or diagnostic, extract it
                        loggerTemplate.TypedResources.Loggers.Add(logger);
                    }
                }
            }

            return loggerTemplate;
        }

        async Task LoadAllReferencedLoggers(
            List<string> apisToExtract, 
            ExtractorParameters extractorParameters)
        {
            var serviceDiagnostics = await this.diagnosticClient.GetAllAsync(extractorParameters);

            foreach (var serviceDiagnostic in serviceDiagnostics)
            {
                string loggerId = serviceDiagnostic.Properties.LoggerId;

                var serviceDiagnosticsKey = ParameterNamingHelper.GenerateValidParameterName(serviceDiagnostic.Name, ParameterPrefix.Diagnostic);

                if (!this.Cache.ServiceLevelDiagnosticLoggerBindings.ContainsKey(serviceDiagnosticsKey))
                {
                    this.Cache.ServiceLevelDiagnosticLoggerBindings.Add(serviceDiagnosticsKey, loggerId);
                }
            }

            if (apisToExtract.IsNullOrEmpty())
            {
                this.logger.LogWarning("No apis to extract are passed to {0}", nameof(LoggerExtractor));
                return;
            }

            foreach (string curApiName in apisToExtract)
            {
                var diagnostics = await this.diagnosticClient.GetApiDiagnosticsAsync(curApiName, extractorParameters);

                if (diagnostics.IsNullOrEmpty())
                {
                    this.logger.LogWarning("No diagnostics found for '{0}' api", curApiName);
                    continue;
                }

                var diagnosticLoggerBindings = new HashSet<DiagnosticLoggerBinding>();
                foreach (var diagnostic in diagnostics)
                {
                    diagnosticLoggerBindings.Add(new DiagnosticLoggerBinding
                    {
                        DiagnosticName = ParameterNamingHelper.GenerateValidParameterName(diagnostic.Name, ParameterPrefix.Diagnostic),
                        LoggerId = diagnostic.Properties.LoggerId
                    });
                }

                if (!diagnosticLoggerBindings.IsNullOrEmpty())
                {
                    this.Cache.ApiDiagnosticLoggerBindings.Add(
                        ParameterNamingHelper.GenerateValidParameterName(curApiName, ParameterPrefix.Api), 
                        diagnosticLoggerBindings);
                }
            }
        }
    }
}
