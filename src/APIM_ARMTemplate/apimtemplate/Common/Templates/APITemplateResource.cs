using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class APITemplateResource: TemplateResource
    {
        public APITemplateProperties properties { get; set; }
        public APITemplateSubResource[] resources { get; set; }
    }

    public class APITemplateProperties
    {
        public string description { get; set; }
        public APITemplateAuthenticationSettings authenticationSettings { get; set; }
        public APITemplateSubscriptionKeyParameterNames subscriptionKeyParameterNames { get; set; }
        public string type { get; set; }
        public string apiRevision { get; set; }
        public string apiVersion { get; set; }
        public string apiRevisionDescription { get; set; }
        public string apiVersionDescription { get; set; }
        public string apiVersionSetId { get; set; }
        // subscriptionRequired is not available for version 2018-01-01
        //public bool subscriptionRequired { get; set; }
        public string displayName { get; set; }
        public string serviceUrl { get; set; }
        public string path { get; set; }
        public string[] protocols { get; set; }
        public APITemplateVersionSet apiVersionSet { get; set; }
        public string contentValue { get; set; }
        public string contentFormat { get; set; }
        public APITemplateWSDLSelector wsdlSelector { get; set; }
        public string apiType { get; set; }
    }

    public class APITemplateAuthenticationSettings
    {
        public APITemplateOAuth2 oAuth2 { get; set; }
        public APITemplateOpenID openid { get; set; }
        public bool subscriptionKeyRequired { get; set; }
    }

    public class APITemplateSubscriptionKeyParameterNames
    {
        public string header { get; set; }
        public string query { get; set; }
    }

    public class APITemplateVersionSet
    {
        public string id { get; set; }
        public string description { get; set; }
        public string versioningScheme { get; set; }
        public string versionQueryName { get; set; }
        public string versionHeaderName { get; set; }
    }

    public class APITemplateWSDLSelector
    {
        public string wsdlServiceName { get; set; }
        public string wsdlEndpointName { get; set; }
    }

    public class APITemplateOAuth2
    {
        public string authorizationServerId { get; set; }
        public string scope { get; set; }
    }

    public class APITemplateOpenID
    {
        public string openidProviderId { get; set; }
        public string[] bearerTokenSendingMethods { get; set; }
    }

    public abstract class APITemplateSubResource : TemplateResource { }

}