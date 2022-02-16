﻿// --------------------------------------------------------------------------
//  <copyright file="ExtractorE2ETestsBase.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions
{
    public abstract class ExtractorTestsBase
    {
        protected const string MockSourceApimName = "test-source-apim-instance-name";
        protected const string MockDestinationApimName = "test-destination-apim-name";
        protected const string MockResourceGroup = "test-resource-group";
        protected const string MockFileFolder = "test-file-folder";
        protected const string MockApiName = "test-api-name";
        protected const string MockMultipleApis = " test-multiple-api-1, test-multiple-api-2 ";
        protected const string MockLinkedTemplatesBaseUrl = "test-linked-templates-base-url";
        protected const string MockLinkedTemplatesSasToken = "test-linked-templates-sas-token";
        protected const string MockLinkedTemplatesUrlQueryString = "test-linked-templates-url-query-string";
        protected const string MockPolicyXMLBaseUrl = "test-policy-xml-base-url";
        protected const string MockPolicyXMLSasToken = "test-policy-xml-sas-token";
        protected const bool MockSplitApis = true;
        protected const string MockApiVersionSetName = "test-api-version-set-name";
        protected const bool MockIncludeAllRevisions = true;
        protected const string MockBaseFileName = "test-base-file-name";
        protected static ServiceUrlProperty[] MockServiceUrlParameters = new[] { new ServiceUrlProperty("test-service-url-property-api-name", "test-service-url-property-url") };
        protected const bool MockToParameterizeServiceUrl = true;
        protected const bool MockToParameterizeNamedValue = true;
        protected const bool MockToParameterizeApiLoggerId = true;
        protected const bool MockToParameterizeLogResourceId = true;
        protected const string MockServiceBaseUrl = "test-service-base-url";
        protected const bool MockNotIncludeNamedValue = true;
        protected const bool MockToParameterizeNamedValuesKeyVaultSecrets = true;
        protected const int MockOperationBatchSize = 32;
        protected const bool MockToParameterizeBackend = true;

        protected ExtractorConsoleAppConfiguration GetDefaultExtractorConfiguration() => new ExtractorConsoleAppConfiguration
        {
            SourceApimName = MockSourceApimName,
            DestinationApimName = MockDestinationApimName,
            ResourceGroup = MockResourceGroup,
            FileFolder = MockFileFolder,
            ApiName = MockApiName,
            MultipleAPIs = MockMultipleApis,
            LinkedTemplatesBaseUrl = MockLinkedTemplatesBaseUrl,
            LinkedTemplatesSasToken = MockLinkedTemplatesSasToken,
            LinkedTemplatesUrlQueryString = MockLinkedTemplatesUrlQueryString,
            PolicyXMLBaseUrl = MockPolicyXMLBaseUrl,
            PolicyXMLSasToken = MockPolicyXMLSasToken,
            SplitAPIs = MockSplitApis.ToString(),
            ApiVersionSetName = MockApiVersionSetName,
            IncludeAllRevisions = MockIncludeAllRevisions.ToString(),
            BaseFileName = MockBaseFileName,
            ServiceUrlParameters = MockServiceUrlParameters,
            ParamServiceUrl = MockToParameterizeServiceUrl.ToString(),
            ParamNamedValue = MockToParameterizeNamedValue.ToString(),
            ParamApiLoggerId = MockToParameterizeApiLoggerId.ToString(),
            ParamLogResourceId = MockToParameterizeLogResourceId.ToString(),
            ServiceBaseUrl = MockServiceBaseUrl,
            NotIncludeNamedValue = MockNotIncludeNamedValue.ToString(),
            ParamNamedValuesKeyVaultSecrets = MockToParameterizeNamedValuesKeyVaultSecrets.ToString(),
            OperationBatchSize = MockOperationBatchSize,
            ParamBackend = MockToParameterizeBackend.ToString()
        };
    }
}
