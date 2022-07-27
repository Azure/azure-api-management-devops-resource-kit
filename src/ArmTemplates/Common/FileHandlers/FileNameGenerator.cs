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
                ApiVersionSetsParameters = $@"{baseFileName}apiVersionSets.parameters.json",
                AuthorizationServers = $@"{baseFileName}authorizationServers.template.json",
                AuthorizationServersParameters = $@"{baseFileName}authorizationServers.parameters.json",
                Backends = $@"{baseFileName}backends.template.json",
                BackendsParameters = $@"{baseFileName}backends.parameters.json",
                GlobalServicePolicy = $@"{baseFileName}globalServicePolicy.template.json",
                GlobalServicePolicyParameters = $@"{baseFileName}globalServicePolicy.parameters.json",
                Loggers = $@"{baseFileName}loggers.template.json",
                LoggersParameters = $@"{baseFileName}loggers.parameters.json",
                NamedValues = $@"{baseFileName}namedValues.template.json",
                NamedValuesParameters = $@"{baseFileName}namedValues.parameters.json",
                Tags = $@"{baseFileName}tags.template.json",
                TagsParameters = $@"{baseFileName}tags.parameters.json",
                Products = $@"{baseFileName}products.template.json",
                ProductsParameters = $@"{baseFileName}products.parameters.json",
                ProductAPIs = $@"{baseFileName}productAPIs.template.json",
                ProductAPIsParameters = $@"{baseFileName}productAPIs.parameters.json",
                Gateway = $@"{baseFileName}gateways.template.json",
                GatewayParameters = $@"{baseFileName}gateways.parameters.json",
                GatewayApi = $@"{baseFileName}gateways-apis.template.json",
                GatewayApiParameters = $@"{baseFileName}gateways-apis.parameters.json",
                IdentityProviders = $@"{baseFileName}identity-providers.template.json",
                IdentityProvidersParameters = $@"{baseFileName}identity-providers.parameters.json",
                OpenIdConnectProviders = $@"{baseFileName}openid-connect-providers.template.json",
                OpenIdConnectProvidersParameters = $@"{baseFileName}openid-connect-providers.parameters.json",
                ApiManagementService = $@"{baseFileName}api-management-service.template.json",
                TagApi = $@"{baseFileName}apiTags.template.json",
                TagApiParameters = $@"{baseFileName}apiTags.parameters.json",
                Schema = $@"{baseFileName}schemas.template.json",
                SchemaParameters = $@"{baseFileName}schemas.parameters.json",
                PolicyFragments = $@"{baseFileName}policy-fragments.template.json",
                PolicyFragmentsParameters = $@"{baseFileName}policy-fragments.parameters.json",
                Parameters = $@"{baseFileName}parameters.json",
                LinkedMaster = $@"{baseFileName}master.template.json",
                Apis = "/Apis",
                SplitAPIs = "/SplitAPIs",
                VersionSetMasterFolder = "/VersionSetMasterFolder",
                RevisionMasterFolder = "/RevisionMasterFolder",
                GroupAPIsMasterFolder = "/MultipleApisMasterFolder",
                Groups = $"{baseFileName}groups.template.json",
                GroupsParameters = $"{baseFileName}groups.parameters.json",
                ParametersDirectory = "parameters",
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
            return singleAPIName == null ? $@"{baseFileName}apis.template.json" : $@"{baseFileName}{singleAPIName}-api.template.json";
        }

        public static string GenerateOriginalAPIName(string apiName)
        {
            // in case the api name has been appended with ;rev={revisionNumber}, take only the api name initially provided by the user in the creator config
            string originalName = apiName.Split(";")[0];
            return originalName;
        }
    }
}
