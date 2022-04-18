// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class ProductAPITemplateCreator
    {
        readonly ITemplateBuilder templateBuilder;

        public ProductAPITemplateCreator(ITemplateBuilder templateBuilder)
        {
            this.templateBuilder = templateBuilder;
        }

        public Template CreateProductAPITemplate(CreatorConfig creatorConfig)
        {
            // create empty template
            Template productTemplate = this.templateBuilder.GenerateEmptyTemplate().Build();

            // add parameters
            productTemplate.Parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ Type = "string" } }
            };

            List<TemplateResource> resources = new List<TemplateResource>();
            string[] dependsOn = new string[] { };
            foreach (APIConfig api in creatorConfig.apis)
            {
                if (api.products != null)
                {
                    List<ProductApiTemplateResource> apiResources = this.CreateProductAPITemplateResources(api, dependsOn);
                    resources.AddRange(apiResources);

                    // Add previous product/API resource as a dependency for next product/API resource(s)
                    string productID = apiResources[apiResources.Count - 1].Name.Split('/', 3)[1];
                    dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/products/apis', parameters('{ParameterNames.ApimServiceName}'), '{productID}', '{api.name}')]" };
                }
            }

            productTemplate.Resources = resources.ToArray();
            return productTemplate;
        }
        public ProductApiTemplateResource CreateProductAPITemplateResource(string productID, string apiName, string[] dependsOn)
        {
            // create products/apis resource with properties
            ProductApiTemplateResource productAPITemplateResource = new ProductApiTemplateResource()
            {
                Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{productID}/{apiName}')]",
                Type = ResourceTypeConstants.ProductApi,
                ApiVersion = GlobalConstants.ApiVersion,
                DependsOn = dependsOn
            };
            return productAPITemplateResource;
        }
        public List<ProductApiTemplateResource> CreateProductAPITemplateResources(APIConfig api, string[] dependsOn)
        {
            // create a products/apis association resource for each product provided in the config file
            List<ProductApiTemplateResource> productAPITemplates = new List<ProductApiTemplateResource>();
            // products is comma separated list of productIds
            string[] productIDs = (api.products ?? "").Split(", ", System.StringSplitOptions.RemoveEmptyEntries);
            string[] allDependsOn = dependsOn;
            foreach (string productID in productIDs)
            {
                ProductApiTemplateResource productAPITemplate = this.CreateProductAPITemplateResource(productID, api.name, allDependsOn);
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
