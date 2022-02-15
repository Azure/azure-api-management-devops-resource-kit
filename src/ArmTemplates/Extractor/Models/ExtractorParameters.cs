namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models
{
    public class ExtractorParameters
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
        public bool notIncludeNamedValue { get; private set; }
        public bool paramNamedValuesKeyVaultSecrets { get; private set; }

        public int operationBatchSize { get; private set; }

        public bool paramBackend { get; set; }

        public ExtractorParameters(ExtractorConfig exc, string dirName)
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
            this.paramServiceUrl = exc.paramServiceUrl != null && exc.paramServiceUrl.Equals("true") || exc.serviceUrlParameters != null;
            this.paramNamedValue = exc.paramNamedValue != null && exc.paramNamedValue.Equals("true");
            this.paramApiLoggerId = exc.paramApiLoggerId != null && exc.paramApiLoggerId.Equals("true");
            this.paramLogResourceId = exc.paramLogResourceId != null && exc.paramLogResourceId.Equals("true");
            this.notIncludeNamedValue = exc.notIncludeNamedValue != null && exc.notIncludeNamedValue.Equals("true");
            this.operationBatchSize = exc.operationBatchSize;
            this.paramNamedValuesKeyVaultSecrets = exc.paramNamedValuesKeyVaultSecrets != null && exc.paramNamedValuesKeyVaultSecrets.Equals("true");
            this.paramBackend = exc.paramBackend != null && exc.paramBackend.Equals("true");
        }

        public ExtractorParameters(ExtractorConfig exc) : this(exc, exc.fileFolder)
        {
        }
    }
}
