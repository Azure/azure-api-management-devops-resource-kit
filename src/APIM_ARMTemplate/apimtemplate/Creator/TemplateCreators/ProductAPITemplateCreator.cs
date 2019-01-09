using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class ProductAPITemplateCreator
    {
        private TemplateCreator templateCreator;

        public ProductAPITemplateCreator(TemplateCreator templateCreator)
        {
            this.templateCreator = templateCreator;
        }

        public Template CreateProductAPITemplate(CreatorConfig creatorConfig)
        {
            Template productAPITemplate = this.templateCreator.CreateEmptyTemplate();

            List<TemplateResource> resources = new List<TemplateResource>();
            // create products/apis resource with properties
            ProductAPITemplateResource productAPITemplateResource = new ProductAPITemplateResource()
            {
                type = "Microsoft.ApiManagement/service/products/apis",
                apiVersion = "2018-06-01-preview",
                properties = new ProductAPITemplateProperties()
            };
            resources.Add(productAPITemplateResource);

            productAPITemplate.resources = resources.ToArray();
            return productAPITemplate;
        }
    }
}
