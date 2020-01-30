using System;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class LoggerExtractor : EntityExtractor
    {
        public async Task<string> GetLoggersAsync(string ApiManagementName, string ResourceGroupName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/loggers?api-version={4}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetLoggerDetailsAsync(string ApiManagementName, string ResourceGroupName, string loggerName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/loggers/{4}?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, loggerName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<Template> GenerateLoggerTemplateAsync(Extractor exc, string singleApiName, List<TemplateResource> apiTemplateResources, Dictionary<string, Dictionary<string, string>> apiLoggerId)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting loggers from service");
            Template armTemplate = GenerateEmptyTemplateWithParameters(exc.policyXMLBaseUrl, exc.policyXMLSasToken);

            // isolate product api associations in the case of a single api extraction
            var policyResources = apiTemplateResources.Where(resource => (resource.type == ResourceTypeConstants.APIPolicy || resource.type == ResourceTypeConstants.APIOperationPolicy || resource.type == ResourceTypeConstants.ProductPolicy));

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // pull all loggers for service
            string loggers = await GetLoggersAsync(exc.sourceApimName, exc.resourceGroup);
            JObject oLoggers = JObject.Parse(loggers);
            foreach (var extractedLogger in oLoggers["value"])
            {
                string loggerName = ((JValue)extractedLogger["name"]).Value.ToString();
                string fullLoggerResource = await GetLoggerDetailsAsync(exc.sourceApimName, exc.resourceGroup, loggerName);

                // convert returned logger to template resource class
                LoggerTemplateResource loggerResource = JsonConvert.DeserializeObject<LoggerTemplateResource>(fullLoggerResource);
                loggerResource.name = $"[concat(parameters('ApimServiceName'), '/{loggerName}')]";
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
                    string validApiName = ExtractorUtils.GenValidParamName(singleApiName, "Api");
                    if (apiLoggerId.ContainsKey(validApiName))
                    {
                        Dictionary<string, string> curDiagnostic = apiLoggerId[validApiName];
                        string validDName = ExtractorUtils.GenValidParamName(loggerResource.properties.loggerType, "Diagnostic").ToLower();
                        if (curDiagnostic.ContainsKey(validDName) && curDiagnostic[validDName].Contains(loggerName))
                        {
                            isReferencedInDiagnostic = true;
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
