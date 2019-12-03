using System.Collections.Generic;

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
        public string includeAllRevisions { get; set; }
        public List<string> multipleAPINames { get; set; }
    }

    public class Extractor
    {
        public string sourceApimName { get; private set; }
        public string destinationApimName { get; private set; }
        public string resourceGroup { get; private set; }
        public string fileFolder { get; private set; }
        public string apiName { get; private set; }
        public string linkedTemplatesBaseUrl { get; private set; }
        public string linkedTemplatesUrlQueryString { get; private set; }
        public string policyXMLBaseUrl { get; private set; }
        public string apiVersionSetName { get; private set; }
        public string includeAllRevisions { get; private set; }
        public List<string> multipleAPINames { get; private set; }

        public Extractor(ExtractorConfig exc, List<string> multipleAPINames, string dirName)
        {
            this.sourceApimName = exc.sourceApimName;
            this.destinationApimName = exc.destinationApimName;
            this.resourceGroup = exc.resourceGroup;
            this.fileFolder = dirName;
            this.apiName = null;
            this.linkedTemplatesBaseUrl = exc.linkedTemplatesBaseUrl;
            this.linkedTemplatesUrlQueryString = exc.linkedTemplatesUrlQueryString;
            this.policyXMLBaseUrl = exc.policyXMLBaseUrl;
            this.apiVersionSetName = exc.apiVersionSetName;
            this.includeAllRevisions = exc.includeAllRevisions;
            if (exc.multipleAPINames != null) {
                this.multipleAPINames = new List<string>(exc.multipleAPINames);
            }
        }

        public Extractor(ExtractorConfig exc, string singleApiName, string dirName)
        {
            this.sourceApimName = exc.sourceApimName;
            this.destinationApimName = exc.destinationApimName;
            this.resourceGroup = exc.resourceGroup;
            this.fileFolder = dirName;
            this.apiName = singleApiName;
            this.linkedTemplatesBaseUrl = exc.linkedTemplatesBaseUrl;
            this.linkedTemplatesUrlQueryString = exc.linkedTemplatesUrlQueryString;
            this.policyXMLBaseUrl = exc.policyXMLBaseUrl;
            this.apiVersionSetName = exc.apiVersionSetName;
            this.includeAllRevisions = exc.includeAllRevisions;
            this.multipleAPINames = null;
        }

        public Extractor(ExtractorConfig exc, string dirName)
        {
            this.sourceApimName = exc.sourceApimName;
            this.destinationApimName = exc.destinationApimName;
            this.resourceGroup = exc.resourceGroup;
            this.fileFolder = dirName;
            this.apiName = exc.apiName;
            this.linkedTemplatesBaseUrl = exc.linkedTemplatesBaseUrl;
            this.linkedTemplatesUrlQueryString = exc.linkedTemplatesUrlQueryString;
            this.policyXMLBaseUrl = exc.policyXMLBaseUrl;
            this.apiVersionSetName = exc.apiVersionSetName;
            this.includeAllRevisions = exc.includeAllRevisions;
            if (exc.multipleAPINames != null) {
                this.multipleAPINames = new List<string>(exc.multipleAPINames);
            }
        }

        public Extractor(ExtractorConfig exc)
        {
            this.sourceApimName = exc.sourceApimName;
            this.destinationApimName = exc.destinationApimName;
            this.resourceGroup = exc.resourceGroup;
            this.fileFolder = exc.fileFolder;
            this.apiName = exc.apiName;
            this.linkedTemplatesBaseUrl = exc.linkedTemplatesBaseUrl;
            this.linkedTemplatesUrlQueryString = exc.linkedTemplatesUrlQueryString;
            this.policyXMLBaseUrl = exc.policyXMLBaseUrl;
            this.apiVersionSetName = exc.apiVersionSetName;
            this.includeAllRevisions = exc.includeAllRevisions;
            if (exc.multipleAPINames != null) {
                this.multipleAPINames = new List<string>(exc.multipleAPINames);
            }
        }
    }
}