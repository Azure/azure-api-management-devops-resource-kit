using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class ProductApiExtractor : ApiExtractor, IProductApiExtractor
    {
        public async Task<List<TemplateResource>> GenerateSingleProductAPIResourceAsync(string apiName, ExtractorParameters extractorParameters, string[] dependsOn)
        {
            List<TemplateResource> templateResources = new List<TemplateResource>();
            string apimname = extractorParameters.SourceApimName, resourceGroup = extractorParameters.ResourceGroup, fileFolder = extractorParameters.FilesGenerationRootDirectory, policyXMLBaseUrl = extractorParameters.PolicyXMLBaseUrl, policyXMLSasToken = extractorParameters.PolicyXMLSasToken;

            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting products from {0} API:", apiName);

            // add product api associations to template
            #region API Products
            try
            {
                // pull product api associations
                string apiProducts = await this.GetAPIProductsAsync(apimname, resourceGroup, apiName);
                JObject oApiProducts = JObject.Parse(apiProducts);

                string lastProductAPIName = null;

                foreach (var item in oApiProducts["value"])
                {
                    string apiProductName = ((JValue)item["name"]).Value.ToString();
                    Console.WriteLine("'{0}' Product association found", apiProductName);

                    // convert returned api product associations to template resource class
                    ProductAPITemplateResource productAPIResource = JsonConvert.DeserializeObject<ProductAPITemplateResource>(item.ToString());
                    productAPIResource.Type = ResourceTypeConstants.ProductAPI;
                    productAPIResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiProductName}/{apiName}')]";
                    productAPIResource.ApiVersion = GlobalConstants.ApiVersion;
                    productAPIResource.Scale = null;
                    productAPIResource.DependsOn = lastProductAPIName != null ? new string[] { lastProductAPIName } : dependsOn;

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
            Template armTemplate = TemplateCreator.GenerateEmptyPropertyTemplateWithParameters();
            List<TemplateResource> templateResources = new List<TemplateResource>();
            // when extract single API
            if (singleApiName != null)
            {
                // check if this api exist
                try
                {
                    string apiDetails = await this.GetAPIDetailsAsync(extractorParameters.SourceApimName, extractorParameters.ResourceGroup, singleApiName);
                    Console.WriteLine("{0} API found ...", singleApiName);
                    templateResources.AddRange(await this.GenerateSingleProductAPIResourceAsync(singleApiName, extractorParameters, new string[] { }));
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
                    templateResources.AddRange(await this.GenerateSingleProductAPIResourceAsync(apiName, extractorParameters, dependsOn));
                    string apiProductName = templateResources.Last().Name.Split('/', 3)[1];
                    dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/products/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiProductName}', '{apiName}')]" };
                }
            }
            // when extract all APIs and generate one master template
            else
            {
                JToken[] oApis = await this.GetAllApiObjsAsync(extractorParameters.SourceApimName, extractorParameters.ResourceGroup);
                Console.WriteLine("{0} APIs found ...", oApis.Count().ToString());

                string[] dependsOn = new string[] { };
                foreach (JToken oApi in oApis)
                {
                    string apiName = ((JValue)oApi["name"]).Value.ToString();
                    templateResources.AddRange(await this.GenerateSingleProductAPIResourceAsync(apiName, extractorParameters, dependsOn));

                    if (templateResources.Count > 0)
                    {
                        string apiProductName = templateResources.Last().Name.Split('/', 3)[1];
                        dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/products/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiProductName}', '{apiName}')]" };
                    }
                }
            }
            armTemplate.Resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}