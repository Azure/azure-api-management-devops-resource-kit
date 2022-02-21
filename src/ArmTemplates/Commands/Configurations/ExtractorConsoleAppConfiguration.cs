using System;
using System.ComponentModel;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Configurations
{
    public class ExtractorConsoleAppConfiguration
    {
        [Description("Config file of the extractor")]
        public string ExtractorConfig { get; set; }

        [Description("Source API Management name")]
        public string SourceApimName { get; set; }

        [Description("Destination API Management name")]
        public string DestinationApimName { get; set; }

        [Description("Resource Group name")]
        public string ResourceGroup { get; set; }

        [Description("ARM Template files folder")]
        public string FileFolder { get; set; }

        [Description("Single API name")]
        public string ApiName { get; set; }

        [Description("Comma-separated list of API names")]
        public string MultipleAPIs { get; set; }

        [Description("Creates a master template with links")]
        public string LinkedTemplatesBaseUrl { get; set; }

        public string LinkedTemplatesSasToken { get; set; }

        [Description("Query string appended to linked templates uris that enables retrieval from private storage")]
        public string LinkedTemplatesUrlQueryString { get; set; }

        [Description("Writes policies to local XML files that require deployment to remote folder")]
        public string PolicyXMLBaseUrl { get; set; }

        [Description("String appended to end of the linked templates uris that enables adding a SAS token or other query parameters")]
        public string PolicyXMLSasToken { get; set; }

        [Description("Split APIs into multiple templates")]
        public string SplitAPIs { get; set; }

        [Description("Name of the apiVersionSet you want to extract")]
        public string ApiVersionSetName { get; set; }

        [Description("Includes all revisions for a single api - use with caution")]
        public string IncludeAllRevisions { get; set; }

        [Description("Specify base name of the template file")]
        public string BaseFileName { get; set; }

        [Description("ServiceUrl parameters")]
        public ServiceUrlProperty[] ServiceUrlParameters { get; set; }

        [Description("Parameterize serviceUrl")]
        public string ParamServiceUrl { get; set; }

        [Description("Parameterize named values")]
        public string ParamNamedValue { get; set; }

        [Description("Specify the loggerId for this api")]
        public string ParamApiLoggerId { get; set; }

        [Description("Specify the resourceId for this logger")]
        public string ParamLogResourceId { get; set; }

        [Description("Specify the the base url for calling api management")]
        public string ServiceBaseUrl { get; set; }

        [Description("Should not include named values template")]
        public string NotIncludeNamedValue { get; set; }

        [Description("Parameterize named values where value is retrieved from a Key Vault secret")]
        public string ParamNamedValuesKeyVaultSecrets { get; set; }

        [Description("Group the operations into batches of x?")]
        public int OperationBatchSize { get; set; }

        [Description("Parameterize environment specific values from backend")]
        public string ParamBackend { get; set; }

        public void Validate(bool ignorePreviousValidations = false)
        {
            if (string.IsNullOrEmpty(this.SourceApimName)) throw new ArgumentException("Missing parameter <sourceApimName>.");
            if (string.IsNullOrEmpty(this.DestinationApimName)) throw new ArgumentException("Missing parameter <destinationApimName>.");
            if (string.IsNullOrEmpty(this.ResourceGroup)) throw new ArgumentException("Missing parameter <resourceGroup>.");
            if (string.IsNullOrEmpty(this.FileFolder)) throw new ArgumentException("Missing parameter <fileFolder>.");

            bool shouldSplitAPIs = this.SplitAPIs != null && this.SplitAPIs.Equals("true");
            bool hasVersionSetName = this.ApiVersionSetName != null;
            bool hasSingleApi = this.ApiName != null;
            bool includeRevisions = this.IncludeAllRevisions != null && this.IncludeAllRevisions.Equals("true");
            bool hasMultipleAPIs = this.MultipleAPIs != null;

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
}