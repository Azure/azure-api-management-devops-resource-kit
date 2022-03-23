using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class PropertyExtractor : EntityExtractorBase, IPropertyExtractor
    {
        readonly IPolicyExtractor policyExtractor;
        readonly ITemplateBuilder templateBuilder;

        public PropertyExtractor(
            ITemplateBuilder templateBuilder,
            IPolicyExtractor policyExtractor)
        {
            this.templateBuilder = templateBuilder;
            this.policyExtractor = policyExtractor;
        }

        public async Task<string[]> GetPropertiesAsync(string apiManagementName, string resourceGroupName)
        {
            JObject oProperty = new JObject();
            int numOfProperties = 0;
            List<string> propertyObjs = new List<string>();
            do
            {
                (string azToken, string azSubId) = await this.Auth.GetAccessToken();
                string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/namedValues?$skip={4}&api-version={5}",
                   BaseUrl, azSubId, resourceGroupName, apiManagementName, numOfProperties, GlobalConstants.ApiVersion);

                numOfProperties += GlobalConstants.NumOfRecords;

                string properties = await this.CallApiManagementAsync(azToken, requestUrl);

                oProperty = JObject.Parse(properties);

                foreach (var item in oProperty["value"])
                {
                    propertyObjs.Add(item.ToString());
                }
            }
            while (oProperty["nextLink"] != null);

            return propertyObjs.ToArray();
        }

        public async Task<string> GetPropertyDetailsAsync(string apiManagementName, string resourceGroupName, string propertyName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/namedValues/{4}?api-version={5}",
               BaseUrl, azSubId, resourceGroupName, apiManagementName, propertyName, GlobalConstants.ApiVersion);

            return await this.CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<Template> GenerateNamedValuesTemplateAsync(
            string singleApiName, 
            List<TemplateResource> apiTemplateResources, 
            ExtractorParameters extractorParameters, 
            IBackendExtractor backendExtractor, 
            List<TemplateResource> loggerTemplateResources,
            string baseFilesGenerationDirectory)
        {
            Template armTemplate = this.templateBuilder.GenerateTemplateWithApimServiceNameProperty().Build();

            if (extractorParameters.ParameterizeNamedValue)
            {
                TemplateParameterProperties namedValueParameterProperties = new TemplateParameterProperties()
                {
                    type = "object"
                };
                armTemplate.Parameters.Add(ParameterNames.NamedValues, namedValueParameterProperties);
            }
            if (extractorParameters.ParamNamedValuesKeyVaultSecrets)
            {
                TemplateParameterProperties keyVaultNamedValueParameterProperties = new TemplateParameterProperties()
                {
                    type = "object"
                };
                armTemplate.Parameters.Add(ParameterNames.NamedValueKeyVaultSecrets, keyVaultNamedValueParameterProperties);
            }
            if (extractorParameters.NotIncludeNamedValue == true)
            {
                Console.WriteLine("------------------------------------------");
                Console.WriteLine("Skipping extracting named values from service");
                return armTemplate;
            }

            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting named values from service");

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // pull all named values (properties) for service
            string[] properties = await this.GetPropertiesAsync(extractorParameters.SourceApimName, extractorParameters.ResourceGroup);

            // isolate api and operation policy resources in the case of a single api extraction, as they may reference named value
            var policyResources = apiTemplateResources.Where(resource => resource.Type == ResourceTypeConstants.APIPolicy || resource.Type == ResourceTypeConstants.APIOperationPolicy || resource.Type == ResourceTypeConstants.ProductPolicy);

            foreach (var extractedProperty in properties)
            {
                JToken oProperty = JObject.Parse(extractedProperty);
                string propertyName = ((JValue)oProperty["name"]).Value.ToString();
                string fullPropertyResource = await this.GetPropertyDetailsAsync(extractorParameters.SourceApimName, extractorParameters.ResourceGroup, propertyName);

                // convert returned named value to template resource class
                PropertyTemplateResource propertyTemplateResource = fullPropertyResource.Deserialize<PropertyTemplateResource>();
                propertyTemplateResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{propertyName}')]";
                propertyTemplateResource.Type = ResourceTypeConstants.Property;
                propertyTemplateResource.ApiVersion = GlobalConstants.ApiVersion;
                propertyTemplateResource.Scale = null;

                if (extractorParameters.ParameterizeNamedValue)
                {
                    propertyTemplateResource.Properties.value = $"[parameters('{ParameterNames.NamedValues}').{ParameterNamingHelper.GenerateValidParameterName(propertyName, ParameterPrefix.Property)}]";
                }

                //Hide the value field if it is a keyvault named value
                if (propertyTemplateResource.Properties.keyVault != null)
                {
                    propertyTemplateResource.Properties.value = null;
                }

                if (propertyTemplateResource.Properties.keyVault != null && extractorParameters.ParamNamedValuesKeyVaultSecrets)
                {
                    propertyTemplateResource.Properties.keyVault.secretIdentifier = $"[parameters('{ParameterNames.NamedValueKeyVaultSecrets}').{ParameterNamingHelper.GenerateValidParameterName(propertyName, ParameterPrefix.Property)}]";
                }

                if (singleApiName == null)
                {
                    // if the user is executing a full extraction, extract all the loggers
                    Console.WriteLine("'{0}' Named value found", propertyName);
                    templateResources.Add(propertyTemplateResource);
                }
                else
                {
                    // if the user is executing a single api, extract all the named values used in the template resources
                    bool foundInPolicy = this.DoesPolicyReferenceNamedValue(extractorParameters, policyResources, propertyName, propertyTemplateResource, baseFilesGenerationDirectory);
                    bool foundInBackEnd = await backendExtractor.IsNamedValueUsedInBackends(extractorParameters.SourceApimName, extractorParameters.ResourceGroup, singleApiName, apiTemplateResources, extractorParameters, propertyName, propertyTemplateResource.Properties.displayName, baseFilesGenerationDirectory);
                    bool foundInLogger = this.DoesLoggerReferenceNamedValue(loggerTemplateResources, propertyName, propertyTemplateResource);

                    // check if named value is referenced in a backend
                    if (foundInPolicy || foundInBackEnd || foundInLogger)
                    {
                        // named value was used in policy, extract it
                        Console.WriteLine("'{0}' Named value found", propertyName);
                        templateResources.Add(propertyTemplateResource);
                    }
                }
            }

            armTemplate.Resources = templateResources.ToArray();
            return armTemplate;
        }

        bool DoesPolicyReferenceNamedValue(
            ExtractorParameters extractorParameters, 
            IEnumerable<TemplateResource> policyResources, 
            string propertyName, 
            PropertyTemplateResource propertyTemplateResource,
            string baseFilesGenerationDirectory)
        {
            // check if named value is referenced in a policy file
            foreach (PolicyTemplateResource policyTemplateResource in policyResources)
            {
                var policyContent = this.policyExtractor.GetCachedPolicyContent(policyTemplateResource, baseFilesGenerationDirectory);

                if (policyContent.Contains(string.Concat("{{", propertyTemplateResource.Properties.displayName, "}}")) || policyContent.Contains(string.Concat("{{", propertyName, "}}")))
                {
                    // dont need to go through all policies if the named value has already been found
                    return true;
                }
            }
            return false;
        }

        bool DoesLoggerReferenceNamedValue(IEnumerable<TemplateResource> loggerTemplateResources, string propertyName, PropertyTemplateResource propertyTemplateResource)
        {
            foreach (LoggerTemplateResource logger in loggerTemplateResources)
            {
                if (logger.Properties.credentials != null)
                {
                    if (!string.IsNullOrEmpty(logger.Properties.credentials.connectionString) && logger.Properties.credentials.connectionString.Contains(propertyName) ||
                        !string.IsNullOrEmpty(logger.Properties.credentials.instrumentationKey) && logger.Properties.credentials.instrumentationKey.Contains(propertyName) ||
                        !string.IsNullOrEmpty(logger.Properties.credentials.connectionString) && logger.Properties.credentials.connectionString.Contains(propertyTemplateResource.Properties.displayName) ||
                        !string.IsNullOrEmpty(logger.Properties.credentials.instrumentationKey) && logger.Properties.credentials.instrumentationKey.Contains(propertyTemplateResource.Properties.displayName))
                    {
                        // dont need to go through all loggers if the named value has already been found
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
