using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apimtemplate.Common.Constants;
using apimtemplate.Common.TemplateModels;
using apimtemplate.Common.Templates.Abstractions;
using apimtemplate.Extractor.EntityExtractors.Abstractions;
using apimtemplate.Extractor.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace apimtemplate.Extractor.EntityExtractors
{
    public class ProductApiExtractor : ApiExtractor, IProductApiExtractor
    {
        public async Task<List<TemplateResource>> GenerateSingleProductAPIResourceAsync(string apiName, ExtractorParameters extractorParameters, string[] dependsOn)
        {
            List<TemplateResource> templateResources = new List<TemplateResource>();
            string apimname = extractorParameters.sourceApimName, resourceGroup = extractorParameters.resourceGroup, fileFolder = extractorParameters.fileFolder, policyXMLBaseUrl = extractorParameters.policyXMLBaseUrl, policyXMLSasToken = extractorParameters.policyXMLSasToken;

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
                    productAPIResource.dependsOn = lastProductAPIName != null ? new string[] { lastProductAPIName } : dependsOn;

                    templateResources.Add(productAPIResource);

                    lastProductAPIName = $"[resourceId('Microsoft.ApiManagement/service/products/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiProductName}', '{apiName}')]";
                }
            }
            catch (Exception) { }
            #endregion

            return templateResources;
        }

        public async Task<Template> GenerateAPIProductsARMTemplateAsync(string singleApiName, List<string> multipleApiNames, ExtractorParameters extractorParameters)
        {
            // initialize arm template
            Template armTemplate = GenerateEmptyPropertyTemplateWithParameters();
            List<TemplateResource> templateResources = new List<TemplateResource>();
            // when extract single API
            if (singleApiName != null)
            {
                // check if this api exist
                try
                {
                    string apiDetails = await GetAPIDetailsAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup, singleApiName);
                    Console.WriteLine("{0} API found ...", singleApiName);
                    templateResources.AddRange(await GenerateSingleProductAPIResourceAsync(singleApiName, extractorParameters, new string[] { }));
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

                string[] dependsOn = new string[] { };
                foreach (string apiName in multipleApiNames)
                {
                    templateResources.AddRange(await GenerateSingleProductAPIResourceAsync(apiName, extractorParameters, dependsOn));
                    string apiProductName = templateResources.Last().name.Split('/', 3)[1];
                    dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/products/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiProductName}', '{apiName}')]" };
                }
            }
            // when extract all APIs and generate one master template
            else
            {
                JToken[] oApis = await GetAllApiObjsAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup);
                Console.WriteLine("{0} APIs found ...", oApis.Count().ToString());

                string[] dependsOn = new string[] { };
                foreach (JToken oApi in oApis)
                {
                    string apiName = ((JValue)oApi["name"]).Value.ToString();
                    templateResources.AddRange(await GenerateSingleProductAPIResourceAsync(apiName, extractorParameters, dependsOn));

                    if (templateResources.Count > 0)
                    {
                        string apiProductName = templateResources.Last().name.Split('/', 3)[1];
                        dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/products/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiProductName}', '{apiName}')]" };
                    }
                }
            }
            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}