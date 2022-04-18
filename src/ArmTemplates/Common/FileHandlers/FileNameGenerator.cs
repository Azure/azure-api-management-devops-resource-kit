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
                AuthorizationServers = $@"{baseFileName}authorizationServers.template.json",
                Backends = $@"{baseFileName}backends.template.json",
                GlobalServicePolicy = $@"{baseFileName}globalServicePolicy.template.json",
                Loggers = $@"{baseFileName}loggers.template.json",
                NamedValues = $@"{baseFileName}namedValues.template.json",
                Tags = $@"{baseFileName}tags.template.json",
                Products = $@"{baseFileName}products.template.json",
                ProductAPIs = $@"{baseFileName}productAPIs.template.json",
                Gateway = $@"{baseFileName}gateways.template.json",
                GatewayApi = $@"{baseFileName}gateways-apis.template.json",
                TagApi = $@"{baseFileName}apiTags.template.json",
                Parameters = $@"{baseFileName}parameters.json",
                LinkedMaster = $@"{baseFileName}master.template.json",
                Apis = "/Apis",
                SplitAPIs = "/SplitAPIs",
                VersionSetMasterFolder = "/VersionSetMasterFolder",
                RevisionMasterFolder = "/RevisionMasterFolder",
                GroupAPIsMasterFolder = "/MultipleApisMasterFolder",
                Groups = $"{baseFileName}groups.template.json",
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
