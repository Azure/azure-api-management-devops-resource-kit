using System;
using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models
{
    public record ExtractorParameters
    {
        public string SourceApimName { get; private set; }

        public string DestinationApimName { get; private set; }

        public string ResourceGroup { get; private set; }

        /// <summary>
        /// Naming of all templates from the base folder requested by end-user in extractor-configuration parameters
        /// </summary>
        public FileNames FileNames { get; private set; }

        /// <summary>
        /// The root directory, where templates will be generated.
        /// </summary>
        public string FilesGenerationRootDirectory { get; private set; }

        /// <summary>
        /// Name of a single API that user wants to extract
        /// </summary>
        public string SingleApiName { get; private set; }

        /// <summary>
        /// Names of APIs that user wants to extract
        /// </summary>
        public List<string> MultipleApiNames { get; private set; }

        /// <summary>
        /// Create separate api templates for every API in the source APIM instance
        /// </summary>
        public bool SplitApis { get; private set; }

        /// <summary>
        /// Are revisions included to templates
        /// </summary>
        public bool IncludeAllRevisions { get; private set; }

        public string LinkedTemplatesBaseUrl { get; private set; }

        public string LinkedTemplatesSasToken { get; private set; }

        public string LinkedTemplatesUrlQueryString { get; private set; }

        public string PolicyXMLBaseUrl { get; private set; }

        public string PolicyXMLSasToken { get; private set; }

        public string ApiVersionSetName { get; private set; }

        public ServiceUrlProperty[] ServiceUrlParameters { get; private set; }

        public bool ParameterizeServiceUrl { get; private set; }

        public bool ParameterizeNamedValue { get; private set; }

        public bool ParameterizeApiLoggerId { get; private set; }

        public bool ParameterizeLogResourceId { get; private set; }

        public bool NotIncludeNamedValue { get; private set; }

        public bool ParamNamedValuesKeyVaultSecrets { get; private set; }

        public int OperationBatchSize { get; private set; }

        public bool ParameterizeBackend { get; private set; }

        public ExtractorParameters(ExtractorConsoleAppConfiguration extractorConfig)
        {
            this.SourceApimName = extractorConfig.SourceApimName;
            this.DestinationApimName = extractorConfig.DestinationApimName;
            this.ResourceGroup = extractorConfig.ResourceGroup;
            this.FilesGenerationRootDirectory = extractorConfig.FileFolder;
            this.SingleApiName = extractorConfig.ApiName;
            this.LinkedTemplatesBaseUrl = extractorConfig.LinkedTemplatesBaseUrl;
            this.LinkedTemplatesSasToken = extractorConfig.LinkedTemplatesSasToken;
            this.LinkedTemplatesUrlQueryString = extractorConfig.LinkedTemplatesUrlQueryString;
            this.PolicyXMLBaseUrl = extractorConfig.PolicyXMLBaseUrl;
            this.PolicyXMLSasToken = extractorConfig.PolicyXMLSasToken;
            this.ApiVersionSetName = extractorConfig.ApiVersionSetName;
            this.IncludeAllRevisions = extractorConfig.IncludeAllRevisions != null && extractorConfig.IncludeAllRevisions.Equals("true", StringComparison.OrdinalIgnoreCase);
            this.ServiceUrlParameters = extractorConfig.ServiceUrlParameters;
            this.ParameterizeServiceUrl = extractorConfig.ParamServiceUrl != null && extractorConfig.ParamServiceUrl.Equals("true", StringComparison.OrdinalIgnoreCase) || extractorConfig.ServiceUrlParameters != null;
            this.ParameterizeNamedValue = extractorConfig.ParamNamedValue != null && extractorConfig.ParamNamedValue.Equals("true", StringComparison.OrdinalIgnoreCase);
            this.ParameterizeApiLoggerId = extractorConfig.ParamApiLoggerId != null && extractorConfig.ParamApiLoggerId.Equals("true", StringComparison.OrdinalIgnoreCase);
            this.ParameterizeLogResourceId = extractorConfig.ParamLogResourceId != null && extractorConfig.ParamLogResourceId.Equals("true", StringComparison.OrdinalIgnoreCase);
            this.NotIncludeNamedValue = extractorConfig.NotIncludeNamedValue != null && extractorConfig.NotIncludeNamedValue.Equals("true", StringComparison.OrdinalIgnoreCase);
            this.OperationBatchSize = extractorConfig.OperationBatchSize;
            this.ParamNamedValuesKeyVaultSecrets = extractorConfig.ParamNamedValuesKeyVaultSecrets != null && extractorConfig.ParamNamedValuesKeyVaultSecrets.Equals("true", StringComparison.OrdinalIgnoreCase);
            this.ParameterizeBackend = extractorConfig.ParamBackend != null && extractorConfig.ParamBackend.Equals("true", StringComparison.OrdinalIgnoreCase);

            this.SplitApis = !string.IsNullOrEmpty(extractorConfig.SplitAPIs) && extractorConfig.SplitAPIs.Equals("true", StringComparison.OrdinalIgnoreCase);
            this.IncludeAllRevisions = !string.IsNullOrEmpty(extractorConfig.IncludeAllRevisions) && extractorConfig.IncludeAllRevisions.Equals("true", StringComparison.OrdinalIgnoreCase);
            this.FileNames = this.GenerateFileNames(extractorConfig.BaseFileName, extractorConfig.SourceApimName);
            this.MultipleApiNames = this.ParseMultipleApiNames(extractorConfig.MultipleAPIs);
        }

        List<string> ParseMultipleApiNames(string multipleApisArgument)
        {
            if (string.IsNullOrEmpty(multipleApisArgument)) return null;            

            var validApiNames = new List<string>();
            foreach (var apiName in multipleApisArgument.Split(','))
            {
                var trimmedApiName = apiName.Trim();
                if (!string.IsNullOrEmpty(trimmedApiName))
                {
                    validApiNames.Add(trimmedApiName);
                }
            }

            return validApiNames;
        }

        FileNames GenerateFileNames(string userRequestedGenerationBaseFolder, string sourceApimInstanceName)
            => string.IsNullOrEmpty(userRequestedGenerationBaseFolder)  
                ? FileNameGenerator.GenerateFileNames(sourceApimInstanceName) 
                : FileNameGenerator.GenerateFileNames(userRequestedGenerationBaseFolder);
        
    }
}
