using apimtemplate.Common.Templates.Abstractions;
using System.Collections.Generic;
namespace apimtemplate.Common.TemplateModels
{
    public class ProductGroupsTemplateResource
    {
        public List<ProductGroupsValue> value { get; set; }
    }

    public class ProductGroupsValue : TemplateResource
    {
        public ProductGroupTemplateProperties properties { get; set; }
    }

    public class ProductGroupTemplateProperties
    {
        public string description { get; set; }
        public string displayName { get; set; }
        public string type { get; set; }
        public string externalId { get; set; }
        public bool builtIn { get; set; }
    }
}