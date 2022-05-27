// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Configurations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions
{
    public abstract class ExtractorMockerTestsBase : TestsBase
    {
        protected const string MockSourceApimName = "source-apim-name";
        protected const string MockDestinationApimName = "destination-apim-name";
        protected const string MockResourceGroup = "resource-group";
        protected const string MockFileFolder = "test-file-folder";
        protected const string MockApiName = "echo-api";
        protected const string MockMultipleApis = " test-multiple-api-1, test-multiple-api-2 ";
        protected const string MockLinkedTemplatesBaseUrl = "test-linked-templates-base-url";
        protected const string MockLinkedTemplatesSasToken = "test-linked-templates-sas-token";
        protected const string MockLinkedTemplatesUrlQueryString = "test-linked-templates-url-query-string";
        protected const string MockPolicyXMLBaseUrl = "";
        protected const string MockPolicyXMLSasToken = "";
        protected const bool MockSplitApis = true;
        protected const string MockApiVersionSetName = "test-api-version-set-name";
        protected const bool MockIncludeAllRevisions = true;
        protected const string MockBaseFileName = "test-base-file-name";
        protected static ServiceUrlProperty[] MockServiceUrlParameters = new[] { new ServiceUrlProperty("test-service-url-property-api-name", "test-service-url-property-url") };
        protected static Dictionary<string, ApiParameterProperty> MockApiParameters = new Dictionary<string, ApiParameterProperty> { { "api-name-1", new ApiParameterProperty("oauth2-scope-value", "test-service-url") } };
        protected const bool MockParameterizeServiceUrl = true;
        protected const bool MockParameterizeNamedValue = true;
        protected const bool MockParameterizeApiLoggerId = true;
        protected const bool MockParameterizeLogResourceId = true;
        protected const string MockServiceBaseUrl = "test-service-base-url";
        protected const bool MockNotIncludeNamedValue = true;
        protected const bool MockToParameterizeNamedValuesKeyVaultSecrets = true;
        protected const int MockOperationBatchSize = 32;
        protected const bool MockParameterizeBackendSettings = true;
        protected const bool MockExtractGateways = true;

        protected ExtractorConsoleAppConfiguration GetMockedExtractorConsoleAppConfiguration(
            bool splitApis = MockSplitApis,
            string apiVersionSetName = MockApiVersionSetName,
            string multipleApiNames = MockMultipleApis,
            bool includeAllRevisions = MockIncludeAllRevisions,
            bool toParameterizeApiLoggerId = MockParameterizeApiLoggerId,
            bool toNotIncludeNamedValue = MockNotIncludeNamedValue,
            string policyXmlBaseUrl = MockPolicyXMLBaseUrl,
            string policyXmlSasToken = MockPolicyXMLSasToken)
        {

            return new ExtractorConsoleAppConfiguration
            {
                SourceApimName = MockSourceApimName,
                DestinationApimName = MockDestinationApimName,
                ResourceGroup = MockResourceGroup,
                FileFolder = MockFileFolder,
                ApiName = MockApiName,
                MultipleAPIs = multipleApiNames,
                LinkedTemplatesBaseUrl = MockLinkedTemplatesBaseUrl,
                LinkedTemplatesSasToken = MockLinkedTemplatesSasToken,
                LinkedTemplatesUrlQueryString = MockLinkedTemplatesUrlQueryString,
                PolicyXMLBaseUrl = policyXmlBaseUrl,
                PolicyXMLSasToken = policyXmlSasToken,
                SplitAPIs = splitApis.ToString(),
                ApiVersionSetName = apiVersionSetName,
                IncludeAllRevisions = includeAllRevisions.ToString(),
                BaseFileName = MockBaseFileName,
                ParamServiceUrl = MockParameterizeServiceUrl.ToString(),
                ParamNamedValue = MockParameterizeNamedValue.ToString(),
                ParamApiLoggerId = toParameterizeApiLoggerId.ToString(),
                ParamLogResourceId = MockParameterizeLogResourceId.ToString(),
                ServiceBaseUrl = MockServiceBaseUrl,
                NotIncludeNamedValue = toNotIncludeNamedValue.ToString(),
                ParamNamedValuesKeyVaultSecrets = MockToParameterizeNamedValuesKeyVaultSecrets.ToString(),
                OperationBatchSize = MockOperationBatchSize,
                ParamBackend = MockParameterizeBackendSettings.ToString(),
                ExtractGateways = MockExtractGateways.ToString(),
                ApiParameters = MockApiParameters
            };
        }

        protected ExtractorConsoleAppConfiguration GetDefaultExtractorConsoleAppConfiguration(
               string sourceApimName = MockSourceApimName,
               string destinationApimName = MockDestinationApimName,
               string resourceGroup = MockResourceGroup,
               string fileFolder = MockFileFolder,
               string apiName = MockApiName,
               string multipleAPIs = null,
               string linkedTemplatesBaseUrl = null,
               string linkedTemplatesSasToken = null,
               string linkedTemplatesUrlQueryString = null,
               string policyXmlBaseUrl = null,
               string policyXmlSasToken = null,
               string splitAPIs = null,
               string apiVersionSetName = null,
               string includeAllRevisions = null,
               string baseFileName = null,
               string paramServiceUrl = null,
               string paramNamedValue = null,
               string paramApiLoggerId = null,
               string paramLogResourceId = null,
               string serviceBaseUrl = null,
               string notIncludeNamedValue = null,
               string paramNamedValuesKeyVaultSecrets = null,
               int? operationBatchSize = null,
               string paramBackend = null,
               string extractGateways = null,
               string overrideGroupGuids = null,
               string overrideProductGuids = null,
               string paramApiOauth2Scope = null,
               Dictionary<string, ApiParameterProperty> apiParameters = null
         )
        {

            return new ExtractorConsoleAppConfiguration
            {
                SourceApimName = sourceApimName,
                DestinationApimName = destinationApimName,
                ResourceGroup = resourceGroup,
                FileFolder = fileFolder,
                ApiName = apiName,
                MultipleAPIs = multipleAPIs,
                LinkedTemplatesBaseUrl = linkedTemplatesBaseUrl,
                LinkedTemplatesSasToken = linkedTemplatesSasToken,
                LinkedTemplatesUrlQueryString = linkedTemplatesUrlQueryString,
                PolicyXMLBaseUrl = policyXmlBaseUrl,
                PolicyXMLSasToken = policyXmlSasToken,
                SplitAPIs = splitAPIs,
                ApiVersionSetName = apiVersionSetName,
                IncludeAllRevisions = includeAllRevisions,
                BaseFileName = baseFileName,
                ParamServiceUrl = paramServiceUrl,
                ParamNamedValue = paramNamedValue,
                ParamApiLoggerId = paramApiLoggerId,
                ParamLogResourceId = paramLogResourceId,
                ServiceBaseUrl = serviceBaseUrl,
                NotIncludeNamedValue = notIncludeNamedValue,
                ParamNamedValuesKeyVaultSecrets = paramNamedValuesKeyVaultSecrets,
                OperationBatchSize = operationBatchSize,
                ParamBackend = paramBackend,
                ExtractGateways = extractGateways,
                OverrideGroupGuids = overrideGroupGuids,
                OverrideProductGuids = overrideProductGuids,
                ParamApiOauth2Scope = paramApiOauth2Scope,
                ApiParameters = apiParameters
            };
        }
    }
}
