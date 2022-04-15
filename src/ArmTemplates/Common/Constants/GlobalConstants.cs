// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants
{
    public static class GlobalConstants
    {
        public static string CommandStartDateTime = "";
        public const string AppShortName = "apimtemplate";
        public const string AppLongName = "API Management DevOps Toolkit";
        public const string AppDescription = "API Management DevOps Toolkit is a tool to create API Management definitions from files and to extract existing API Management APIs to files.";
        public const string CreateName = "create";
        public const string CreateDescription = "Create an API Management instance from files";
        public const string ExtractName = "extract";
        public const string ExtractDescription = "Extract an existing API Management instance";

        public const string ApiVersion = "2021-08-01";
        public const string ArmApiVersion = "2022-01-01";
        public const int NumOfRecords = 100;

        public const string azAccessToken = "account get-access-token --query \"accessToken\" --output json";
        public const string azSubscriptionId = "account show --query id -o json";

        public static string BaseManagementAzureUrl = "https://management.azure.com";
    }

    public static class ParameterNames
    {
        public const string ApiLoggerId = "apiLoggerId";
        public const string NamedValues = "namedValues";
        public const string LoggerResourceId = "loggerResourceId";
        public const string ServiceUrl = "serviceUrl";
        public const string PolicyXMLSasToken = "policyXMLSasToken";
        public const string PolicyXMLBaseUrl = "policyXMLBaseUrl";
        public const string LinkedTemplatesUrlQueryString = "linkedTemplatesUrlQueryString";
        public const string LinkedTemplatesSasToken = "linkedTemplatesSasToken";
        public const string ApimServiceName = "apimServiceName";
        public const string LinkedTemplatesBaseUrl = "linkedTemplatesBaseUrl";
        public const string NamedValueKeyVaultSecrets = "namedValueKeyVaultSecrets";
        public const string BackendSettings = "backendSettings";
    }

    public static class ParameterPrefix
    {
        public const string Api = "Api";
        public const string Diagnostic = "Diagnostic";
        public const string Property = "Property";
        public const string LogResourceId = "LogResourceId";
        public const string Backend = "Backend";
    }
}