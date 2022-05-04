// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using CommandLine;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Configurations
{
    [Verb(GlobalConstants.ExtractName, HelpText = GlobalConstants.ExtractDescription)]
    public class ExtractorConsoleAppConfiguration
    {
        [Option(longName: "extractorConfig", HelpText = "Config file of the extractor")]
        public string ExtractorConfig { get; set; }

        [Option(longName: "sourceApimName", HelpText = "Source API Management name")]
        public string SourceApimName { get; set; }

        [Option(longName: "destinationApimName", HelpText = "Destination API Management name")]
        public string DestinationApimName { get; set; }

        [Option(longName: "resourceGroup", HelpText = "Resource Group name")]
        public string ResourceGroup { get; set; }

        [Option(longName: "fileFolder", HelpText = "ARM Template files folder")]
        public string FileFolder { get; set; }

        [Option(longName: "apiName", HelpText = "Single API name")]
        public string ApiName { get; set; }

        [Option(longName: "multipleAPIs", HelpText = "Comma-separated list of API names")]
        public string MultipleAPIs { get; set; }

        [Option(longName: "linkedTemplatesBaseUrl", HelpText = "Creates a master template with links")]
        public string LinkedTemplatesBaseUrl { get; set; }

        [Option(longName: "linkedTemplatesSasToken", HelpText = "Creates a master template with links")]
        public string LinkedTemplatesSasToken { get; set; }

        [Option(longName: "linkedTemplatesUrlQueryString", HelpText = "Query string appended to linked templates uris that enables retrieval from private storage")]
        public string LinkedTemplatesUrlQueryString { get; set; }

        [Option(longName: "policyXMLBaseUrl", HelpText = "Writes policies to local XML files that require deployment to remote folder")]
        public string PolicyXMLBaseUrl { get; set; }

        [Option(longName: "policyXMLSasToken", HelpText = "String appended to end of the linked templates uris that enables adding a SAS token or other query parameters")]
        public string PolicyXMLSasToken { get; set; }

        [Option(longName: "splitAPIs", HelpText = "Split APIs into multiple templates")]
        public string SplitAPIs { get; set; }

        [Option(longName: "apiVersionSetName", HelpText = "Name of the apiVersionSet you want to extract")]
        public string ApiVersionSetName { get; set; }

        [Option(longName: "includeAllRevisions", HelpText = "Includes all revisions for a single api - use with caution")]
        public string IncludeAllRevisions { get; set; }

        [Option(longName: "baseFileName", HelpText = "Specify base name of the template file")]
        public string BaseFileName { get; set; }

        // this is used only from json-file and --extractorConfig option, so no option possibility here
        public ServiceUrlProperty[] ServiceUrlParameters { get; set; }

        [Option(longName: "paramServiceUrl", HelpText = "Parameterize serviceUrl")]
        public string ParamServiceUrl { get; set; }

        [Option(longName: "paramNamedValue", HelpText = "Parameterize named values")]
        public string ParamNamedValue { get; set; }

        [Option(longName: "paramApiLoggerId", HelpText = "Specify the loggerId for this api")]
        public string ParamApiLoggerId { get; set; }

        [Option(longName: "paramLogResourceId", HelpText = "Specify the resourceId for this logger")]
        public string ParamLogResourceId { get; set; }

        [Option(longName: "serviceBaseUrl", HelpText = "Specify the the base url for calling api management")]
        public string ServiceBaseUrl { get; set; }

        [Option(longName: "notIncludeNamedValue", HelpText = "Should not include named values template")]
        public string NotIncludeNamedValue { get; set; }

        [Option(longName: "paramNamedValuesKeyVaultSecrets", HelpText = "Parameterize named values where value is retrieved from a Key Vault secret")]
        public string ParamNamedValuesKeyVaultSecrets { get; set; }

        [Option(longName: "operationBatchSize", HelpText = "Group the operations into batches of x?")]
        public int? OperationBatchSize { get; set; }

        [Option(longName: "paramBackend", HelpText = "Parameterize environment specific values from backend")]
        public string ParamBackend { get; set; }

        [Option(longName: "extractGateways", HelpText = "Attempt to extract information about Self-Hosted gateways")]
        public string ExtractGateways { get; set; }
    }
}