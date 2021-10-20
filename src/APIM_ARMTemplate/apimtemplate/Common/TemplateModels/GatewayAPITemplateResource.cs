using System.Collections.Generic;
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    public class GatewayAPITemplateResource : TemplateResource
    {
        public GatewayAPIProperties properties { get; set; }
        public class GatewayAPIProperties
        {  
            public string provisioningState { get; set; }
        }
    }
}