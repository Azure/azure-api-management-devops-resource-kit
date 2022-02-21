using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels
{
    public class ProductsTemplateResource : TemplateResource
    {
        public ProductsTemplateProperties Properties { get; set; }
    }

    public class ProductsTemplateProperties
    {
        public string name { get; set; }
        public string description { get; set; }
        public string terms { get; set; }
        public bool subscriptionRequired { get; set; }
        public bool? approvalRequired { get; set; }
        public int? subscriptionsLimit { get; set; }
        public string state { get; set; }
        public string displayName { get; set; }
    }
}