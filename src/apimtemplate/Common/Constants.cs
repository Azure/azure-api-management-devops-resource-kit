using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    internal static class Constants
    {
        public const string AppShortName = "apimtemplate";
        public const string AppLongName = "API Management Template";
        public const string AppDescription = "API Management Template is a tool to create API Management definitions from files and to extract existing API Management APIs to files.";
        public const string CreateName = "create";
        public const string CreateDescription = "Create an API Management instance from files";
        public const string ExtractName = "extract";
        public const string ExtractDescription = "Extract an existing API Management instance";

        public const string azParameters = "account get-access-token --query \"accessToken\" --output json";
    }
}