using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class ProductAPITemplateCreator : TemplateCreator
    {
        public Template CreateProductAPITemplate(CreatorConfig creatorConfig)
        {
            // create empty template
            Template productTemplate = CreateEmptyTemplate();

            // add parameters
            productTemplate.parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ type = "string" } }
            };

            List<TemplateResource> resources = new List<TemplateResource>();
            string[] dependsOn = new string[] { };
            foreach (APIConfig api in creatorConfig.apis)
            {
                if (api.products != null)
                {
                    List<ProductAPITemplateResource> apiResources = CreateProductAPITemplateResources(api, dependsOn);
                    resources.AddRange(apiResources);

                    // Add previous product/API resource as a dependency for next product/API resource(s)
                    string productID = apiResources[apiResources.Count - 1].name.Split('/', 3)[1];
                    dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/products/apis', parameters('{ParameterNames.ApimServiceName}'), '{productID}', '{api.name}')]" };
                }
            }

            productTemplate.resources = resources.ToArray();
            return productTemplate;
        }
        public ProductAPITemplateResource CreateProductAPITemplateResource(string productID, string apiName, string[] dependsOn)
        {
            // create products/apis resource with properties
            ProductAPITemplateResource productAPITemplateResource = new ProductAPITemplateResource()
            {
                name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{productID}/{apiName}')]",
                type = ResourceTypeConstants.ProductAPI,
                apiVersion = GlobalConstants.APIVersion,
                dependsOn = dependsOn
            };
            return productAPITemplateResource;
        }
        public List<ProductAPITemplateResource> CreateProductAPITemplateResources(APIConfig api, string[] dependsOn)
        {
            // create a products/apis association resource for each product provided in the config file
            List<ProductAPITemplateResource> productAPITemplates = new List<ProductAPITemplateResource>();
            // products is comma separated list of productIds
            string[] productIDs = (api.products ?? "").Split(", ", System.StringSplitOptions.RemoveEmptyEntries);
            string[] allDependsOn = dependsOn;
            foreach (string productID in productIDs)
            {
                ProductAPITemplateResource productAPITemplate = this.CreateProductAPITemplateResource(productID, api.name, allDependsOn);
                // Add previous product/API resource as a dependency for next product/API resource
                allDependsOn = new string[dependsOn.Length + 1];
                dependsOn.CopyTo(allDependsOn, 1);
                allDependsOn[0] = $"[resourceId('Microsoft.ApiManagement/service/products/apis', parameters('{ParameterNames.ApimServiceName}'), '{productID}', '{api.name}')]";
                productAPITemplates.Add(productAPITemplate);
            }
            return productAPITemplates;
        }
    }
}
