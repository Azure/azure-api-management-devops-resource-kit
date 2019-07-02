using System.Collections.Generic;
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    public class ProductsTemplateResource : TemplateResource
    {
        public ProductsTemplateProperties properties { get; set; }
    }

    public class ProductsTemplateProperties
    {
        public string description { get; set; }
        public string terms { get; set; }
        public bool subscriptionRequired { get; set; }
        public bool approvalRequired { get; set; }
        public int subscriptionsLimit { get; set; }
        public string state { get; set; }
        public string displayName { get; set; }

        // This properties should not be serialized if subscriptionRequired is false
        public bool ShouldSerializeapprovalRequired(){
            return this.subscriptionRequired;
        }

        // This properties should not be serialized if subscriptionRequired is false
        public bool ShouldSerializesubscriptionsLimit(){
            return this.subscriptionRequired;
        }

    }
}