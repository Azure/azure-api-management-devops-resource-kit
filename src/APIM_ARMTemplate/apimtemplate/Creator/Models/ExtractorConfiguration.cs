using System.ComponentModel;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class ExtractorConfig
    {
        [Description("Source API Management name")]
        public string sourceApimName { get; set; }
        [Description("Destination API Management name")]
        public string destinationApimName { get; set; }
        [Description("Resource Group name")]
        public string resourceGroup { get; set; }
        [Description("ARM Template files folder")]
        public string fileFolder { get; set; }
        [Description("API name")]
        public string apiName { get; set; }
        public string mutipleAPIs { get; set; }
        [Description("Creates a master template with links")]
        public string linkedTemplatesBaseUrl { get; set; }
        public string linkedTemplatesSasToken { get; set; }
        [Description("Query string appended to linked templates uris that enables retrieval from private storage")]
        public string linkedTemplatesUrlQueryString { get; set; }
        [Description("Writes policies to local XML files that require deployment to remote folder")]
        public string policyXMLBaseUrl { get; set; }
        public string policyXMLSasToken { get; set; }
        [Description("Split APIs into multiple templates")]
        public string splitAPIs { get; set; }
        [Description("Name of the apiVersionSet you want to extract")]
        public string apiVersionSetName { get; set; }
        public string includeAllRevisions { get; set; }
    }

    public class Extractor
    {
        public string sourceApimName { get; private set; }
        public string destinationApimName { get; private set; }
        public string resourceGroup { get; private set; }
        public string fileFolder { get; private set; }
        public string linkedTemplatesBaseUrl { get; private set; }
        public string linkedTemplatesSasToken { get; private set; }
        public string linkedTemplatesUrlQueryString { get; private set; }
        public string policyXMLBaseUrl { get; private set; }
        public string policyXMLSasToken { get; private set; }
        public string apiVersionSetName { get; private set; }
        public bool includeAllRevisions { get; private set; }

        public Extractor(ExtractorConfig exc, string dirName)
        {
            this.sourceApimName = exc.sourceApimName;
            this.destinationApimName = exc.destinationApimName;
            this.resourceGroup = exc.resourceGroup;
            this.fileFolder = dirName;
            this.linkedTemplatesBaseUrl = exc.linkedTemplatesBaseUrl;
            this.linkedTemplatesSasToken = exc.linkedTemplatesSasToken;
            this.linkedTemplatesUrlQueryString = exc.linkedTemplatesUrlQueryString;
            this.policyXMLBaseUrl = exc.policyXMLBaseUrl;
            this.policyXMLSasToken = exc.policyXMLSasToken;
            this.apiVersionSetName = exc.apiVersionSetName;
            this.includeAllRevisions = checkIncludeRevision(exc.includeAllRevisions);
        }

        public Extractor(ExtractorConfig exc)
        {
            this.sourceApimName = exc.sourceApimName;
            this.destinationApimName = exc.destinationApimName;
            this.resourceGroup = exc.resourceGroup;
            this.fileFolder = exc.fileFolder;
            this.linkedTemplatesBaseUrl = exc.linkedTemplatesBaseUrl;
            this.linkedTemplatesSasToken = exc.linkedTemplatesSasToken;
            this.linkedTemplatesUrlQueryString = exc.linkedTemplatesUrlQueryString;
            this.policyXMLBaseUrl = exc.policyXMLBaseUrl;
            this.policyXMLSasToken = exc.policyXMLSasToken;
            this.apiVersionSetName = exc.apiVersionSetName;
            this.includeAllRevisions = checkIncludeRevision(exc.includeAllRevisions);
        }

        public bool checkIncludeRevision(string includeAllRevisions) {
            return includeAllRevisions != null && includeAllRevisions.Equals("true");
        }
    }
}