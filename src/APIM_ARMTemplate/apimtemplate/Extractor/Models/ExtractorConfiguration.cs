using System;
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
        [Description("Comma-separated list of API names")]
        public string multipleAPIs { get; set; }
        [Description("Creates a master template with links")]
        public string linkedTemplatesBaseUrl { get; set; }
        public string linkedTemplatesSasToken { get; set; }
        [Description("Query string appended to linked templates uris that enables retrieval from private storage")]
        public string linkedTemplatesUrlQueryString { get; set; }
        [Description("Writes policies to local XML files that require deployment to remote folder")]
        public string policyXMLBaseUrl { get; set; }
        [Description("String appended to end of the linked templates uris that enables adding a SAS token or other query parameters")]
        public string policyXMLSasToken { get; set; }
        [Description("Split APIs into multiple templates")]
        public string splitAPIs { get; set; }
        [Description("Name of the apiVersionSet you want to extract")]
        public string apiVersionSetName { get; set; }
        [Description("Includes all revisions for a single api - use with caution")]
        public string includeAllRevisions { get; set; }
        [Description("Specify base name of the template file")]
        public string baseFileName { get; set; }
        [Description("ServiceUrl parameters")]
        public serviceUrlProperty[] serviceUrlParameters { get; set; }
        [Description("Parameterize serviceUrl")]
        public string paramServiceUrl { get; set; }
        [Description("Parameterize named values")]
        public string paramNamedValue { get; set; }
        [Description("Specify the loggerId for this api")]
        public string paramApiLoggerId { get; set; }
        [Description("Specify the resourceId for this logger")]
        public string paramLogResourceId { get; set; }
        [Description("Specify the the base url for calling api management")]
        public string serviceBaseUrl { get; set; }
        public void Validate()
        {
            if (string.IsNullOrEmpty(sourceApimName)) throw new ArgumentException("Missing parameter <sourceApimName>.");
            if (string.IsNullOrEmpty(destinationApimName)) throw new ArgumentException("Missing parameter <destinationApimName>.");
            if (string.IsNullOrEmpty(resourceGroup)) throw new ArgumentException("Missing parameter <resourceGroup>.");
            if (string.IsNullOrEmpty(fileFolder)) throw new ArgumentException("Missing parameter <fileFolder>.");

            bool shouldSplitAPIs = splitAPIs != null && splitAPIs.Equals("true");
            bool hasVersionSetName = apiVersionSetName != null;
            bool hasSingleApi = apiName != null;
            bool includeRevisions = includeAllRevisions != null && includeAllRevisions.Equals("true");
            bool hasMultipleAPIs = multipleAPIs != null;

            if (shouldSplitAPIs && hasSingleApi)
            {
                throw new NotSupportedException("Can't use splitAPIs and apiName at same time");
            }

            if (shouldSplitAPIs && hasVersionSetName)
            {
                throw new NotSupportedException("Can't use splitAPIs and apiVersionSetName at same time");
            }

            if ((hasVersionSetName || hasSingleApi) && hasMultipleAPIs)
            {
                throw new NotSupportedException("Can't use multipleAPIs with apiName or apiVersionSetName at the same time");
            }

            if (hasSingleApi && hasVersionSetName)
            {
                throw new NotSupportedException("Can't use apiName and apiVersionSetName at same time");
            }

            if (!hasSingleApi && includeRevisions)
            {
                throw new NotSupportedException("\"includeAllRevisions\" can be used when you specify the API you want to extract with \"apiName\"");
            }
        }
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
        public serviceUrlProperty[] serviceUrlParameters { get; set; }
        public bool paramServiceUrl { get; private set; }
        public bool paramNamedValue { get; private set; }
        public bool paramApiLoggerId { get; private set; }
        public bool paramLogResourceId { get; private set; }

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
            this.includeAllRevisions = exc.includeAllRevisions != null && exc.includeAllRevisions.Equals("true");
            this.serviceUrlParameters = exc.serviceUrlParameters;
            this.paramServiceUrl = (exc.paramServiceUrl != null && exc.paramServiceUrl.Equals("true")) || exc.serviceUrlParameters != null;
            this.paramNamedValue = exc.paramNamedValue != null && exc.paramNamedValue.Equals("true");
            this.paramApiLoggerId = exc.paramApiLoggerId != null && exc.paramApiLoggerId.Equals("true");
            this.paramLogResourceId = exc.paramLogResourceId != null && exc.paramLogResourceId.Equals("true");
        }

        public Extractor(ExtractorConfig exc) : this(exc, exc.fileFolder)
        {
        }
    }

    public class serviceUrlProperty
    {
        public string apiName { get; private set; }
        public string serviceUrl { get; private set; }
        public serviceUrlProperty(string apiName, string serviceUrl)
        {
            this.apiName = apiName;
            this.serviceUrl = serviceUrl;
        }
    }
}