namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions
{
    public class APITemplateResource : TemplateResource
    {
        public APITemplateProperties Properties { get; set; }

        public APITemplateSubResource[] Resources { get; set; }
    }

    public class APITemplateProperties
    {
        public string Description { get; set; }

        public APITemplateAuthenticationSettings AuthenticationSettings { get; set; }

        public APITemplateSubscriptionKeyParameterNames SubscriptionKeyParameterNames { get; set; }

        public string Type { get; set; }

        public string ApiRevision { get; set; }

        public string ApiVersion { get; set; }

        public bool? IsCurrent { get; set; }

        public string ApiRevisionDescription { get; set; }

        public string ApiVersionDescription { get; set; }
        
        public string ApiVersionSetId { get; set; }
        
        public bool? SubscriptionRequired { get; set; }
        
        public string SourceApiId { get; set; }
        
        public string DisplayName { get; set; }
        
        public string ServiceUrl { get; set; }
        
        public string Path { get; set; }
        
        public string[] Protocols { get; set; }
        
        public APITemplateAPIVersionSet ApiVersionSet { get; set; }

        public string Value { get; set; }

        public string Format { get; set; }
        
        public APITemplateWSDLSelector WsdlSelector { get; set; }
        
        public string ApiType { get; set; }
    }

    public class APITemplateAuthenticationSettings
    {
        public APITemplateOAuth2 OAuth2 { get; set; }

        public APITemplateOpenID Openid { get; set; }
        
        public bool SubscriptionKeyRequired { get; set; }
    }

    public class APITemplateSubscriptionKeyParameterNames
    {
        public string Header { get; set; }

        public string Query { get; set; }
    }

    public class APITemplateVersionSet
    {
        public string Id { get; set; }

        public string Description { get; set; }

        public string VersioningScheme { get; set; }

        public string VersionQueryName { get; set; }

        public string VersionHeaderName { get; set; }
    }

    public class APITemplateWSDLSelector
    {
        public string WsdlServiceName { get; set; }

        public string WsdlEndpointName { get; set; }
    }

    public class APITemplateOAuth2
    {
        public string AuthorizationServerId { get; set; }

        public string Scope { get; set; }
    }

    public class APITemplateOpenID
    {
        public string OpenIdProviderId { get; set; }

        public string[] BearerTokenSendingMethods { get; set; }
    }

    public class APITemplateAPIVersionSet
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string VersionQueryName { get; set; }

        public string VersionHeaderName { get; set; }

        public string VersioningScheme { get; set; }
    }

    public abstract class APITemplateSubResource : TemplateResource { }

}