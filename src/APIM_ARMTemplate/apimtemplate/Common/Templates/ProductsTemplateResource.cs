using System.Collections.Generic;
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    public class ProductsProperties
    {
        public string displayName { get; set; }
        public string description { get; set; }
        public string terms { get; set; }
        public bool subscriptionRequired { get; set; }
        public bool approvalRequired { get; set; }
        public int subscriptionsLimit { get; set; }
        public string state { get; set; }
    }

    public class Value
    {
        public string id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public ProductsProperties properties { get; set; }
    }

    public class ProductsTemplateResource
    {
        public List<Value> value { get; set; }
    }

    public class ProductsDetailsTemplateResource : TemplateResource
    {
        public ProductsProperties properties { get; set; }
    }
}