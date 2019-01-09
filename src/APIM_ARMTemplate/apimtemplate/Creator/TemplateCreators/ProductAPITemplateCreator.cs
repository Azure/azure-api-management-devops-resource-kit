using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class ProductAPITemplateCreator
    {
        private TemplateCreator templateCreator;

        public ProductAPITemplateCreator(TemplateCreator templateCreator)
        {
            this.templateCreator = templateCreator;
        }

        public Template CreateProductAPITemplate(string productID)
        {
            Template productAPITemplate = this.templateCreator.CreateEmptyTemplate();

            // add parameters
            productAPITemplate.parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { "ApimServiceName", new TemplateParameterProperties(){ type = "string" } }
            };
            
            List<TemplateResource> resources = new List<TemplateResource>();
            // create products/apis resource with properties
            ProductAPITemplateResource productAPITemplateResource = new ProductAPITemplateResource()
            {
                name = $"[concat(parameters('ApimServiceName'), '/{String.Concat("productapi-", productID)}')]",
                type = "Microsoft.ApiManagement/service/products/apis",
                apiVersion = "2018-06-01-preview",
                properties = new ProductAPITemplateProperties()
            };
            resources.Add(productAPITemplateResource);

            productAPITemplate.resources = resources.ToArray();
            return productAPITemplate;
        }

        public List<Template> CreateProductAPITemplates(CreatorConfig creatorConfig)
        {
            List<Template> productAPITemplates = new List<Template>();
            string[] productIDs = creatorConfig.api.products.Split(", ");
            foreach (string productID in productIDs)
            {
                Template productAPITemplate = this.CreateProductAPITemplate(productID);
                productAPITemplates.Add(productAPITemplate);
            }
            return productAPITemplates;
        }
    }
}
