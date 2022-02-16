using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class LoggerExtractor : EntityExtractorBase, ILoggerExtractor
    {
        public async Task<string> GetLoggersAsync(string apiManagementName, string resourceGroupName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/loggers?api-version={4}",
               BaseUrl, azSubId, resourceGroupName, apiManagementName, GlobalConstants.APIVersion);

            return await this.CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetLoggerDetailsAsync(string apiManagementName, string resourceGroupName, string loggerName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/loggers/{4}?api-version={5}",
               BaseUrl, azSubId, resourceGroupName, apiManagementName, loggerName, GlobalConstants.APIVersion);

            return await this.CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<Template> GenerateLoggerTemplateAsync(ExtractorParameters extractorParameters, string singleApiName, List<TemplateResource> apiTemplateResources, Dictionary<string, object> apiLoggerId)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting loggers from service");
            Template armTemplate = this.GenerateEmptyPropertyTemplateWithParameters();

            if (extractorParameters.ToParameterizeLogResourceId)
            {
                TemplateParameterProperties loggerResourceIdParameterProperties = new TemplateParameterProperties()
                {
                    type = "object"
                };
                armTemplate.parameters.Add(ParameterNames.LoggerResourceId, loggerResourceIdParameterProperties);
            }

            // isolate product api associations in the case of a single api extraction
            var policyResources = apiTemplateResources.Where(resource => resource.type == ResourceTypeConstants.APIPolicy || resource.type == ResourceTypeConstants.APIOperationPolicy || resource.type == ResourceTypeConstants.ProductPolicy);

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // pull all loggers for service
            string loggers = await this.GetLoggersAsync(extractorParameters.SourceApimName, extractorParameters.ResourceGroup);
            JObject oLoggers = JObject.Parse(loggers);
            foreach (var extractedLogger in oLoggers["value"])
            {
                string loggerName = ((JValue)extractedLogger["name"]).Value.ToString();
                string fullLoggerResource = await this.GetLoggerDetailsAsync(extractorParameters.SourceApimName, extractorParameters.ResourceGroup, loggerName);

                // convert returned logger to template resource class
                LoggerTemplateResource loggerResource = JsonConvert.DeserializeObject<LoggerTemplateResource>(fullLoggerResource);
                loggerResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{loggerName}')]";
                loggerResource.type = ResourceTypeConstants.Logger;
                loggerResource.apiVersion = GlobalConstants.APIVersion;
                loggerResource.scale = null;

                if (singleApiName == null)
                {
                    // if the user is extracting all apis, extract all the loggers
                    Console.WriteLine("'{0}' Logger found", loggerName);
                    templateResources.Add(loggerResource);
                }
                else
                {
                    // if the user is extracting a single api, extract the loggers referenced by its diagnostics and api policies
                    bool isReferencedInPolicy = false;
                    bool isReferencedInDiagnostic = false;
                    foreach (PolicyTemplateResource policyTemplateResource in policyResources)
                    {
                        if (policyTemplateResource.properties.value.Contains(loggerName))
                        {
                            isReferencedInPolicy = true;
                        }
                    }
                    string validApiName = ParameterNamingHelper.GenerateValidParameterName(singleApiName, ParameterPrefix.Api);
                    if (extractorParameters.ToParameterizeApiLoggerId && apiLoggerId.ContainsKey(validApiName))
                    {
                        object diagnosticObj = apiLoggerId[validApiName];
                        if (diagnosticObj is Dictionary<string, string>)
                        {
                            Dictionary<string, string> curDiagnostic = (Dictionary<string, string>)diagnosticObj;
                            string validDName = ParameterNamingHelper.GenerateValidParameterName(loggerResource.properties.loggerType, ParameterPrefix.Diagnostic).ToLower();
                            if (curDiagnostic.ContainsKey(validDName) && curDiagnostic[validDName].Contains(loggerName))
                            {
                                isReferencedInDiagnostic = true;
                            }
                        }
                    }
                    if (isReferencedInPolicy == true || isReferencedInDiagnostic == true)
                    {
                        // logger was used in policy or diagnostic, extract it
                        Console.WriteLine("'{0}' Logger found", loggerName);
                        templateResources.Add(loggerResource);
                    }
                };
            }

            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}
