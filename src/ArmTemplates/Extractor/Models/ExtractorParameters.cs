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
            sourceApimName = exc.sourceApimName;
            destinationApimName = exc.destinationApimName;
            resourceGroup = exc.resourceGroup;
            fileFolder = dirName;
            linkedTemplatesBaseUrl = exc.linkedTemplatesBaseUrl;
            linkedTemplatesSasToken = exc.linkedTemplatesSasToken;
            linkedTemplatesUrlQueryString = exc.linkedTemplatesUrlQueryString;
            policyXMLBaseUrl = exc.policyXMLBaseUrl;
            policyXMLSasToken = exc.policyXMLSasToken;
            apiVersionSetName = exc.apiVersionSetName;
            includeAllRevisions = exc.includeAllRevisions != null && exc.includeAllRevisions.Equals("true");
            serviceUrlParameters = exc.serviceUrlParameters;
            paramServiceUrl = exc.paramServiceUrl != null && exc.paramServiceUrl.Equals("true") || exc.serviceUrlParameters != null;
            paramNamedValue = exc.paramNamedValue != null && exc.paramNamedValue.Equals("true");
            paramApiLoggerId = exc.paramApiLoggerId != null && exc.paramApiLoggerId.Equals("true");
            paramLogResourceId = exc.paramLogResourceId != null && exc.paramLogResourceId.Equals("true");
            notIncludeNamedValue = exc.notIncludeNamedValue != null && exc.notIncludeNamedValue.Equals("true");
            operationBatchSize = exc.operationBatchSize;
            paramNamedValuesKeyVaultSecrets = exc.paramNamedValuesKeyVaultSecrets != null && exc.paramNamedValuesKeyVaultSecrets.Equals("true");
            paramBackend = exc.paramBackend != null && exc.paramBackend.Equals("true");
        }

        public ExtractorParameters(ExtractorConfig exc) : this(exc, exc.fileFolder)
        {
        }
    }
}
