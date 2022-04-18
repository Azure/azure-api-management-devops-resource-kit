// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Configurations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions
{
    public abstract class ExtractorMockerTestsBase : TestsBase
    {
        protected const string MockSourceApimName = "dmkorolev-APIM-test";
        protected const string MockDestinationApimName = "test-destination-apim-name";
        protected const string MockResourceGroup = "dmkorolev-test";
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
        protected const bool MockParameterizeServiceUrl = true;
        protected const bool MockParameterizeNamedValue = true;
        protected const bool MockParameterizeApiLoggerId = true;
        protected const bool MockParameterizeLogResourceId = true;
        protected const string MockServiceBaseUrl = "test-service-base-url";
        protected const bool MockNotIncludeNamedValue = true;
        protected const bool MockToParameterizeNamedValuesKeyVaultSecrets = true;
        protected const int MockOperationBatchSize = 32;
        protected const bool MockParameterizeBackend = true;
        protected const bool MockExtractGateways = true;

        protected ExtractorConsoleAppConfiguration GetMockedExtractorConsoleAppConfiguration(
            bool splitApis = MockSplitApis,
            string apiVersionSetName = MockApiVersionSetName,
            string multipleApiNames = MockMultipleApis,
            bool includeAllRevisions = MockIncludeAllRevisions,
            bool toParameterizeApiLoggerId = MockParameterizeApiLoggerId,
            bool toNotIncludeNamedValue = MockNotIncludeNamedValue)
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
                PolicyXMLBaseUrl = MockPolicyXMLBaseUrl,
                PolicyXMLSasToken = MockPolicyXMLSasToken,
                SplitAPIs = splitApis.ToString(),
                ApiVersionSetName = apiVersionSetName,
                IncludeAllRevisions = includeAllRevisions.ToString(),
                BaseFileName = MockBaseFileName,
                ServiceUrlParameters = MockServiceUrlParameters,
                ParamServiceUrl = MockParameterizeServiceUrl.ToString(),
                ParamNamedValue = MockParameterizeNamedValue.ToString(),
                ParamApiLoggerId = toParameterizeApiLoggerId.ToString(),
                ParamLogResourceId = MockParameterizeLogResourceId.ToString(),
                ServiceBaseUrl = MockServiceBaseUrl,
                NotIncludeNamedValue = toNotIncludeNamedValue.ToString(),
                ParamNamedValuesKeyVaultSecrets = MockToParameterizeNamedValuesKeyVaultSecrets.ToString(),
                OperationBatchSize = MockOperationBatchSize,
                ParamBackend = MockParameterizeBackend.ToString(),
                ExtractGateways = MockExtractGateways.ToString()
            };
        }  
    }
}
