
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    public class AuthorizationServerTemplateResource : TemplateResource
    {
        public AuthorizationServerTemplateProperties properties { get; set; }
    }

    public class AuthorizationServerTemplateProperties
    {
        public string description { get; set; }
        public string[] authorizationMethods { get; set; }
        public string[] clientAuthenticationMethod { get; set; }
        public AuthorizationServerTokenBodyParameter[] tokenBodyParameters { get; set; }
        public string tokenEndpoint { get; set; }
        public bool supportState { get; set; }
        public string defaultScope { get; set; }
        public string[] bearerTokenSendingMethods { get; set; }
        public string clientSecret { get; set; }
        public string resourceOwnerUsername { get; set; }
        public string resourceOwnerPassword { get; set; }
        public string displayName { get; set; }
        public string clientRegistrationEndpoint { get; set; }
        public string authorizationEndpoint { get; set; }
        public string[] grantTypes { get; set; }
        public string clientId { get; set; }
    }

    public class AuthorizationServerTokenBodyParameter
    {
        public string name { get; set; }
        public string value { get; set; }
    }

}
