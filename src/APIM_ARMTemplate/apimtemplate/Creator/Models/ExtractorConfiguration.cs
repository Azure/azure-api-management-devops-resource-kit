using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class ExtractorConfig
    {
        public string sourceApimName { get;}
        public string destinationApimName { get;}
        public string resourceGroup { get;}
        public string fileFolder { get;}
        public string apiName { get;}
        public string linkedTemplatesBaseUrl { get;}
        public string linkedTemplatesUrlQueryString { get;}
        public string policyXMLBaseUrl { get;}
        public string splitAPIs { get;}
        public string apiVersionSetName { get;}
        public string includeAllRevisions { get;}
        public List<string> multipleAPINames { get;}

        public ExtractorConfig (ExtractorConfig exc, List<string> multipleAPINames, string dirName) {
            this.sourceApimName = exc.sourceApimName;
            this.destinationApimName = exc.destinationApimName;
            this.resourceGroup = exc.resourceGroup;
            this.fileFolder = dirName;
            this.apiName = null;
            this.linkedTemplatesBaseUrl = exc.linkedTemplatesBaseUrl;
            this.linkedTemplatesUrlQueryString = exc.linkedTemplatesUrlQueryString;
            this.policyXMLBaseUrl = exc.policyXMLBaseUrl;
            this.splitAPIs = exc.splitAPIs;
            this.apiVersionSetName = exc.apiVersionSetName;
            this.includeAllRevisions = exc.includeAllRevisions;
            this.multipleAPINames = new List<string>(multipleAPINames);
        }

        public ExtractorConfig (ExtractorConfig exc, string singleApiName, string dirName) {
            this.sourceApimName = exc.sourceApimName;
            this.destinationApimName = exc.destinationApimName;
            this.resourceGroup = exc.resourceGroup;
            this.fileFolder = dirName;
            this.apiName = singleApiName;
            this.linkedTemplatesBaseUrl = exc.linkedTemplatesBaseUrl;
            this.linkedTemplatesUrlQueryString = exc.linkedTemplatesUrlQueryString;
            this.policyXMLBaseUrl = exc.policyXMLBaseUrl;
            this.splitAPIs = exc.splitAPIs;
            this.apiVersionSetName = exc.apiVersionSetName;
            this.includeAllRevisions = exc.includeAllRevisions;
            this.multipleAPINames = null;
        }

        public ExtractorConfig (ExtractorConfig exc, string dirName) {
            this.sourceApimName = exc.sourceApimName;
            this.destinationApimName = exc.destinationApimName;
            this.resourceGroup = exc.resourceGroup;
            this.fileFolder = dirName;
            this.apiName = exc.apiName;
            this.linkedTemplatesBaseUrl = exc.linkedTemplatesBaseUrl;
            this.linkedTemplatesUrlQueryString = exc.linkedTemplatesUrlQueryString;
            this.policyXMLBaseUrl = exc.policyXMLBaseUrl;
            this.splitAPIs = exc.splitAPIs;
            this.apiVersionSetName = exc.apiVersionSetName;
            this.includeAllRevisions = exc.includeAllRevisions;
            this.multipleAPINames = new List<string>(exc.multipleAPINames);
        }
        
        public ExtractorConfig (ExtractorConfig exc) {
            this.sourceApimName = exc.sourceApimName;
            this.destinationApimName = exc.destinationApimName;
            this.resourceGroup = exc.resourceGroup;
            this.fileFolder = exc.fileFolder;
            this.apiName = exc.apiName;
            this.linkedTemplatesBaseUrl = exc.linkedTemplatesBaseUrl;
            this.linkedTemplatesUrlQueryString = exc.linkedTemplatesUrlQueryString;
            this.policyXMLBaseUrl = exc.policyXMLBaseUrl;
            this.splitAPIs = exc.splitAPIs;
            this.apiVersionSetName = exc.apiVersionSetName;
            this.includeAllRevisions = exc.includeAllRevisions;
            this.multipleAPINames = new List<string>(exc.multipleAPINames);
        }
    }
}