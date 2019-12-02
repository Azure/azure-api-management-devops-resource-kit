namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class ExtractorConfig
    {
        public string sourceApimName { get; set; }
        public string destinationApimName { get; set; }
        public string resourceGroup { get; set; }
        public string fileFolder { get; set; }
        public string apiName { get; set; }
        public string linkedTemplatesBaseUrl { get; set; }
        public string linkedTemplatesUrlQueryString { get; set; }
        public string policyXMLBaseUrl { get; set; }
        public string splitAPIs { get; set; }
        public string apiVersionSetName { get; set; }
    }
}