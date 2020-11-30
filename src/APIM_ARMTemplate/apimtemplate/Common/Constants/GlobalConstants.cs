
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    internal static class GlobalConstants
    {
        public const string AppShortName = "apimtemplate";
        public const string AppLongName = "API Management DevOps Toolkit";
        public const string AppDescription = "API Management DevOps Toolkit is a tool to create API Management definitions from files and to extract existing API Management APIs to files.";
        public const string CreateName = "create";
        public const string CreateDescription = "Create an API Management instance from files";
        public const string ExtractName = "extract";
        public const string ExtractDescription = "Extract an existing API Management instance";

        public const string APIVersion = "2019-01-01";
        public const string LinkedAPIVersion = "2018-05-01";
        public const int NumOfRecords = 100;

        public const string azAccessToken = "account get-access-token --query \"accessToken\" --output json";
        public const string azSubscriptionId = "account show --query id -o json";
    }

    public static class ParameterNames
    {
        public const string ApiLoggerId = "ApiLoggerId";
        public const string NamedValues = "NamedValues";
        public const string LoggerResourceId = "LoggerResourceId";
        public const string ServiceUrl = "serviceUrl";
        public const string PolicyXMLSasToken = "PolicyXMLSasToken";
        public const string PolicyXMLBaseUrl = "PolicyXMLBaseUrl";
        public const string LinkedTemplatesUrlQueryString = "LinkedTemplatesUrlQueryString";
        public const string LinkedTemplatesSasToken = "LinkedTemplatesSasToken";
        public const string ApimServiceName = "ApimServiceName";
        public const string LinkedTemplatesBaseUrl = "LinkedTemplatesBaseUrl";
    }

    public static class ParameterPrefix
    {
        public const string Api = "Api";
        public const string Diagnostic = "Diagnostic";
        public const string Property = "Property";
        public const string LogResourceId = "LogResourceId";
    }
}