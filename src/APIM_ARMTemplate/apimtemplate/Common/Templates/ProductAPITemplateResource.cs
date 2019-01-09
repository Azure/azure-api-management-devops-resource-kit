using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class ProductAPITemplateResource : TemplateResource {
        public ProductAPITemplateProperties properties { get; set; }
    }

    public class ProductAPITemplateProperties { }
}
