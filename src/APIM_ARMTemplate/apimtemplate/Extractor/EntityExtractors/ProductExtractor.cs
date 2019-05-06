using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;
using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class ProductExtractor : EntityExtractor
    {
        public async Task<string> GetProducts(string ApiManagementName, string ResourceGroupName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/products?api-version={4}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, GlobalConstants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }

        public async Task<string> GetProductDetails(string ApiManagementName, string ResourceGroupName, string ProductName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/products/{4}?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ProductName, GlobalConstants.APIVersion);

            return await CallApiManagement(azToken, requestUrl);
        }

        public async Task<Template> GenerateProductsARMTemplate(string apimname, string resourceGroup, string singleApiName, List<TemplateResource> apiTemplateResources)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Getting products from service");
            Template armTemplate = GenerateEmptyTemplateWithParameters();

            // isolate product api associations in the case of a single api extraction
            var productAPIResources = apiTemplateResources.Where(resource => resource.type == ResourceTypeConstants.ProductAPI);

            List<TemplateResource> templateResources = new List<TemplateResource>();

            // pull all products for service
            string products = await GetProducts(apimname, resourceGroup);
            JObject oProducts = JObject.Parse(products);

            foreach (var item in oProducts["value"])
            {
                string productName = ((JValue)item["name"]).Value.ToString();
                string productDetails = await GetProductDetails(apimname, resourceGroup, productName);

                // convert returned product to template resource class
                ProductsTemplateResource productsTemplateResource = JsonConvert.DeserializeObject<ProductsTemplateResource>(productDetails);
                productsTemplateResource.name = $"[concat(parameters('ApimServiceName'), '/{productName}')]";
                productsTemplateResource.apiVersion = GlobalConstants.APIVersion;

                // only extract the product if this is a full extraction, or in the case of a single api, if it is found in products associated with the api
                if (singleApiName == null || productAPIResources.SingleOrDefault(p => p.name.Contains(productName)) != null)
                {
                    Console.WriteLine("'{0}' Product found", productName);
                    templateResources.Add(productsTemplateResource);
                }
            }

            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}
