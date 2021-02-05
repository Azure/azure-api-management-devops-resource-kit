﻿
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    internal static class ResourceTypeConstants
    {
        public const string API = "Microsoft.ApiManagement/service/apis";
        public const string APIVersionSet = "Microsoft.ApiManagement/service/apiVersionSets";
        public const string APIDiagnostic = "Microsoft.ApiManagement/service/apis/diagnostics";
        public const string APIOperation = "Microsoft.ApiManagement/service/apis/operations";
        public const string APIOperationPolicy = "Microsoft.ApiManagement/service/apis/operations/policies";
        public const string APIOperationTag = "Microsoft.ApiManagement/service/apis/operations/tags";
        public const string APITag = "Microsoft.ApiManagement/service/apis/tags";
        public const string APIPolicy = "Microsoft.ApiManagement/service/apis/policies";
        public const string APIRelease = "Microsoft.ApiManagement/service/apis/releases";
        public const string APISchema = "Microsoft.ApiManagement/service/apis/schemas";
        public const string AuthorizationServer = "Microsoft.ApiManagement/service/authorizationServers";
        public const string Backend = "Microsoft.ApiManagement/service/backends";
        public const string GlobalServicePolicy = "Microsoft.ApiManagement/service/policies";
        public const string Logger = "Microsoft.ApiManagement/service/loggers";
        public const string ProductAPI = "Microsoft.ApiManagement/service/products/apis";
        public const string Product = "Microsoft.ApiManagement/service/products";
        public const string ProductGroup = "Microsoft.ApiManagement/service/products/groups";
        public const string ProductTag = "Microsoft.ApiManagement/service/products/tags";
        public const string ProductPolicy = "Microsoft.ApiManagement/service/products/policies";
        public const string Property = "Microsoft.ApiManagement/service/properties";
        public const string Subscription = "Microsoft.ApiManagement/service/subscriptions";
        public const string Tag = "Microsoft.ApiManagement/service/tags";
    }
}