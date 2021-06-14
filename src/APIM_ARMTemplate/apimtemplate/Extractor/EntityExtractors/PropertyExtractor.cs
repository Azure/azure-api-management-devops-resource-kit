﻿using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class PropertyExtractor : EntityExtractor
    {
        public async Task<string[]> GetPropertiesAsync(string ApiManagementName, string ResourceGroupName)
        {
            JObject oProperty = new JObject();
            int numOfProperties = 0;
            List<string> propertyObjs = new List<string>();
            do
            {
                (string azToken, string azSubId) = await auth.GetAccessToken();
                string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/namedValues?$skip={4}&api-version={5}",
                   baseUrl, azSubId, ResourceGroupName, ApiManagementName, numOfProperties, GlobalConstants.APIVersion);

                numOfProperties += GlobalConstants.NumOfRecords;

                string properties = await CallApiManagementAsync(azToken, requestUrl);

                oProperty = JObject.Parse(properties);

                foreach (var item in oProperty["value"])
                {
                    propertyObjs.Add(item.ToString());
                }
            }
            while (oProperty["nextLink"] != null);

            return propertyObjs.ToArray();
        }

        public async Task<string> GetPropertyDetailsAsync(string ApiManagementName, string ResourceGroupName, string propertyName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/namedValues/{4}?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, propertyName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<Template> GenerateNamedValuesTemplateAsync(string singleApiName, List<TemplateResource> apiTemplateResources, Extractor exc)
        {
            if (exc.notIncludeNamedValue == true)
            {
                Console.WriteLine("------------------------------------------");
                Console.WriteLine("Skipping extracting named values from service");
                return GenerateEmptyPropertyTemplateWithParameters(exc);
            }

            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting named values from service");
            Template armTemplate = GenerateEmptyPropertyTemplateWithParameters(exc);

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // pull all named values (properties) for service
            string[] properties = await GetPropertiesAsync(exc.sourceApimName, exc.resourceGroup);

            foreach (var extractedProperty in properties)
            {
                JToken oProperty = JObject.Parse(extractedProperty);
                string propertyName = ((JValue)oProperty["name"]).Value.ToString();
                string fullPropertyResource = await GetPropertyDetailsAsync(exc.sourceApimName, exc.resourceGroup, propertyName);

                // convert returned named value to template resource class
                PropertyTemplateResource propertyTemplateResource = JsonConvert.DeserializeObject<PropertyTemplateResource>(fullPropertyResource);
                propertyTemplateResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{propertyName}')]";
                propertyTemplateResource.type = ResourceTypeConstants.Property;
                propertyTemplateResource.apiVersion = GlobalConstants.APIVersion;
                propertyTemplateResource.scale = null;

                if (exc.paramNamedValue)
                {
                    propertyTemplateResource.properties.value = $"[parameters('{ParameterNames.NamedValues}').{ExtractorUtils.GenValidParamName(propertyName, ParameterPrefix.Property)}]";
                }

                //Hide the value field if it is a keyvault named value
                if (propertyTemplateResource.properties.keyVault != null)
                {
                    propertyTemplateResource.properties.value = null;
                }

                if (propertyTemplateResource.properties.keyVault != null && exc.paramNamedValuesKeyVaultSecrets )
                {
                    propertyTemplateResource.properties.keyVault.secretIdentifier = $"[parameters('{ParameterNames.NamedValueKeyVaultSecrets}').{ExtractorUtils.GenValidParamName(propertyName, ParameterPrefix.Property)}]";
                }

                if (singleApiName == null)
                {
                    // if the user is executing a full extraction, extract all the loggers
                    Console.WriteLine("'{0}' Named value found", propertyName);
                    templateResources.Add(propertyTemplateResource);
                }
                else
                {
                    // TODO - if the user is executing a single api, extract all the named values used in the template resources
                    Console.WriteLine("'{0}' Named value found", propertyName);
                    templateResources.Add(propertyTemplateResource);
                };
            }

            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}
