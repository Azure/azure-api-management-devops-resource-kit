namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    public class GatewayTemplateResource : TemplateResource
    {
        public GatewayProperties properties { get; set; }
    }

    public class GatewayProperties
    {
        public string description { get; set; }
        public GatewayLocationProperties locationData { get; set; }
    }

    public class GatewayLocationProperties
    {
        public string name { get; set; }
        public string city { get; set; }
        public string countryOrRegion { get; set; }
        public string district { get; set; }
    }
}