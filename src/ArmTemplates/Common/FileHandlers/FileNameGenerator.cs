// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers
{
    public static class FileNameGenerator
    {
        const string DefaultBaseFileName = "output-";

        public static FileNames GenerateFileNames(string baseFileName)
        {
            if (string.IsNullOrEmpty(baseFileName))
            {
                baseFileName = DefaultBaseFileName;
            }
            else
            {
                baseFileName += "-";
            }

            // generate useable object with file names for consistency throughout project
            return new FileNames
            {
                ApiVersionSets = $@"{baseFileName}apiVersionSets.template.json",
                ApiVersionSetsParameters = "apiVersionSets.parameters.json",
                AuthorizationServers = $@"{baseFileName}authorizationServers.template.json",
                AuthorizationServersParameters = "authorizationServers.parameters.json",
                Backends = $@"{baseFileName}backends.template.json",
                BackendsParameters = "backends.parameters.json",
                GlobalServicePolicy = $@"{baseFileName}globalServicePolicy.template.json",
                GlobalServicePolicyParameters = "globalServicePolicy.parameters.json",
                Loggers = $@"{baseFileName}loggers.template.json",
                LoggersParameters = "loggers.parameters.json",
                NamedValues = $@"{baseFileName}namedValues.template.json",
                NamedValuesParameters = "namedValues.parameters.json",
                Tags = $@"{baseFileName}tags.template.json",
                TagsParameters = "tags.parameters.json",
                Products = $@"{baseFileName}products.template.json",
                ProductsParameters = "products.parameters.json",
                ProductAPIs = $@"{baseFileName}productAPIs.template.json",
                ProductAPIsParameters = "productAPIs.parameters.json",
                Gateway = $@"{baseFileName}gateways.template.json",
                GatewayParameters = "gateways.parameters.json",
                GatewayApi = $@"{baseFileName}gateways-apis.template.json",
                GatewayApiParameters = "gateways-apis.parameters.json",
                IdentityProviders = $@"{baseFileName}identity-providers.template.json",
                IdentityProvidersParameters = "identity-providers.parameters.json",
                OpenIdConnectProviders = $@"{baseFileName}openid-connect-providers.template.json",
                OpenIdConnectProvidersParameters = "openid-connect-providers.parameters.json",
                ApiManagementService = $@"{baseFileName}api-management-service.template.json",
                TagApi = $@"{baseFileName}apiTags.template.json",
                TagApiParameters = "apiTags.parameters.json",
                Schema = $@"{baseFileName}schemas.template.json",
                SchemaParameters = "schemas.parameters.json",
                PolicyFragments = $@"{baseFileName}policy-fragments.template.json",
                PolicyFragmentsParameters = "policy-fragments.parameters.json",
                Parameters = $@"{baseFileName}parameters.json",
                LinkedMaster = $@"{baseFileName}master.template.json",
                Apis = "/Apis",
                SplitAPIs = "/SplitAPIs",
                VersionSetMasterFolder = "/VersionSetMasterFolder",
                RevisionMasterFolder = "/RevisionMasterFolder",
                GroupAPIsMasterFolder = "/MultipleApisMasterFolder",
                Groups = $"{baseFileName}groups.template.json",
                GroupsParameters = "groups.parameters.json",
                ParametersDirectory = $"{baseFileName}parameters",
                BaseFileName = baseFileName
            };
        }

        public static string GenerateCreatorAPIFileName(string apiName, bool isSplitAPI, bool isInitialAPI)
        {
            // in case the api name has been appended with ;rev={revisionNumber}, take only the api name initially provided by the user in the creator config
            string sanitizedAPIName = GenerateOriginalAPIName(apiName);

            if (isSplitAPI == true)
            {
                return isInitialAPI == true ? $@"/{sanitizedAPIName}-initial.api.template.json" : $@"/{sanitizedAPIName}-subsequent.api.template.json";
            }
            else
            {
                return $@"/{sanitizedAPIName}.api.template.json";
            }
        }

        public static string GenerateExtractorAPIFileName(string singleAPIName, string baseFileName)
        {
            return $@"{baseFileName}{GenerateApiFileNameBase(singleAPIName)}.template.json";
        }

        public static string GenerateExtractorAPIParametersFileName(string singleAPIName)
        {
            return $@"{GenerateApiFileNameBase(singleAPIName)}.parameters.json";
        }

        public static string GenerateApiFileNameBase(string singleAPIName) {
            return singleAPIName == null ? "apis" : $"{singleAPIName}-api";
        }

        public static string GenerateOriginalAPIName(string apiName)
        {
            // in case the api name has been appended with ;rev={revisionNumber}, take only the api name initially provided by the user in the creator config
            string originalName = apiName.Split(";")[0];
            return originalName;
        }
    }
}
