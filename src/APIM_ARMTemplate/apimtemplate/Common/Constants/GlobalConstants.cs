
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    internal static class GlobalConstants
    {
        public const string AppShortName = "apimtemplate";
        public const string AppLongName = "API Management Template";
        public const string AppDescription = "API Management Template is a tool to create API Management definitions from files and to extract existing API Management APIs to files.";
        public const string CreateName = "create";
        public const string CreateDescription = "Create an API Management instance from files";
        public const string ExtractName = "extract";
        public const string ExtractDescription = "Extract an existing API Management instance";

        public const string APIVersion = "2019-01-01";
        public const string LinkedAPIVersion = "2018-01-01";

        public const string azAccessToken = "account get-access-token --query \"accessToken\" --output json";
        public const string azSubscriptionId = "account show --query id -o json";        
    }
}