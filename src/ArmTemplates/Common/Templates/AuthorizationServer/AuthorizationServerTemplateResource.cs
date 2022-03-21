using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.AuthorizationServer
{
    public class AuthorizationServerTemplateResource : TemplateResource
    {
        public AuthorizationServerProperties Properties { get; set; }
    }
}
