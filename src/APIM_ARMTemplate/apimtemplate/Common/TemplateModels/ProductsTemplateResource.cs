﻿using System.Collections.Generic;
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    public class ProductsTemplateResource : TemplateResource
    {
        public ProductsTemplateProperties properties { get; set; }
    }

    public class ProductsTemplateProperties
    {
        private string _name;

        public string description { get; set; }
        public string terms { get; set; }
        public bool subscriptionRequired { get; set; }
        public bool approvalRequired { get; set; }
        public int subscriptionsLimit { get; set; }
        public string state { get; set; }
        public string name { get => _name ?? displayName; set => _name = value; }
        public string displayName { get; set; }
    }
}