﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apimtemplate.Extractor.Utilities;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class ProductAPIExtractor : EntityExtractor
    {
        private FileWriter fileWriter;

        public ProductAPIExtractor(FileWriter fileWriter)
        {
            this.fileWriter = fileWriter;
        }

        public async Task<string> GetAPIDetailsAsync(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<JToken[]> GetAllAPIObjsAsync(string ApiManagementName, string ResourceGroupName)
        {
            JObject oApi = new JObject();
            int numOfApis = 0;
            List<JToken> apiObjs = new List<JToken>();
            do
            {
                (string azToken, string azSubId) = await auth.GetAccessToken();

                string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis?$skip={4}&api-version={5}",
                baseUrl, azSubId, ResourceGroupName, ApiManagementName, numOfApis, GlobalConstants.APIVersion);
                numOfApis += GlobalConstants.NumOfRecords;

                string apis = await CallApiManagementAsync(azToken, requestUrl);

                oApi = JObject.Parse(apis);

                foreach (var item in oApi["value"])
                {
                    apiObjs.Add(item);
                }
            }
            while (oApi["nextLink"] != null);
            return apiObjs.ToArray();
        }

        public async Task<string> GetAPIProductsAsync(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/products?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<List<TemplateResource>> GenerateSingleProductAPIResourceAsync(string apiName, Extractor exc, string[] dependsOn)
        {
            List<TemplateResource> templateResources = new List<TemplateResource>();
            string apimname = exc.sourceApimName, resourceGroup = exc.resourceGroup, fileFolder = exc.fileFolder, policyXMLBaseUrl = exc.policyXMLBaseUrl, policyXMLSasToken = exc.policyXMLSasToken;

            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting products from {0} API:", apiName);

            // add product api associations to template
            #region API Products
            try
            {
                // pull product api associations
                string apiProducts = await GetAPIProductsAsync(apimname, resourceGroup, apiName);
                JObject oApiProducts = JObject.Parse(apiProducts);

                string lastProductAPIName = null;

                foreach (var item in oApiProducts["value"])
                {
                    string apiProductName = ((JValue)item["name"]).Value.ToString();
                    Console.WriteLine("'{0}' Product association found", apiProductName);

                    // convert returned api product associations to template resource class
                    ProductAPITemplateResource productAPIResource = JsonConvert.DeserializeObject<ProductAPITemplateResource>(item.ToString());
                    productAPIResource.type = ResourceTypeConstants.ProductAPI;
                    productAPIResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiProductName}/{apiName}')]";
                    productAPIResource.apiVersion = GlobalConstants.APIVersion;
                    productAPIResource.scale = null;
                    productAPIResource.dependsOn = (lastProductAPIName != null) ? new string[] { lastProductAPIName } : dependsOn;

                    templateResources.Add(productAPIResource);

                    lastProductAPIName = $"[resourceId('Microsoft.ApiManagement/service/products/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiProductName}', '{apiName}')]";
                }
            }
            catch (Exception) { }
            #endregion

            return templateResources;
        }

        public async Task<Template> GenerateAPIProductsARMTemplateAsync(string singleApiName, List<string> multipleApiNames, Extractor exc)
        {
            // initialize arm template
            Template armTemplate = GenerateEmptyApiTemplateWithParameters(exc);
            List<TemplateResource> templateResources = new List<TemplateResource>();
            // when extract single API
            if (singleApiName != null)
            {
                // check if this api exist
                try
                {
                    string apiDetails = await GetAPIDetailsAsync(exc.sourceApimName, exc.resourceGroup, singleApiName);
                    Console.WriteLine("{0} API found ...", singleApiName);
                    templateResources.AddRange(await GenerateSingleProductAPIResourceAsync(singleApiName, exc, new string[] {}));
                }
                catch (Exception)
                {
                    throw new Exception($"{singleApiName} API not found!");
                }
            }
            // when extract multiple APIs and generate one master template
            else if (multipleApiNames != null)
            {
                Console.WriteLine("{0} APIs found ...", multipleApiNames.Count().ToString());

                string[] dependsOn = new string[] {};
                foreach (string apiName in multipleApiNames)
                {
                    templateResources.AddRange(await GenerateSingleProductAPIResourceAsync(apiName, exc, dependsOn));
                    string apiProductName = templateResources.Last().name.Split('/', 3)[1];
                    dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/products/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiProductName}', '{apiName}')]" };
                }
            }
            // when extract all APIs and generate one master template
            else
            {
                JToken[] oApis = await GetAllAPIObjsAsync(exc.sourceApimName, exc.resourceGroup);
                Console.WriteLine("{0} APIs found ...", (oApis.Count().ToString()));

                string[] dependsOn = new string[] {};
                foreach (JToken oApi in oApis)
                {
                    string apiName = ((JValue)oApi["name"]).Value.ToString();
                    templateResources.AddRange(await GenerateSingleProductAPIResourceAsync(apiName, exc, dependsOn));
                    string apiProductName = templateResources.Last().name.Split('/', 3)[1];
                    dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/products/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiProductName}', '{apiName}')]" };
                }
            }
            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}