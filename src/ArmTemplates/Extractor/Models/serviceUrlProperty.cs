namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models
{
    public class ServiceUrlProperty
    {
        public string ApiName { get; private set; }

        public string ServiceUrl { get; private set; }

        public ServiceUrlProperty(string apiName, string serviceUrl)
        {
            this.ApiName = apiName;
            this.ServiceUrl = serviceUrl;
        }
    }
}
