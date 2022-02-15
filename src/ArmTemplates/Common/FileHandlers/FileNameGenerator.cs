namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers
{
    public static class FileNameGenerator
    {

        public static FileNames GenerateFileNames(string baseFileName)
        {
            if (baseFileName.Length > 0)
                baseFileName += "-";

            // generate useable object with file names for consistency throughout project
            return new FileNames()
            {
                apiVersionSets = $@"/{baseFileName}apiVersionSets.template.json",
                authorizationServers = $@"/{baseFileName}authorizationServers.template.json",
                backends = $@"/{baseFileName}backends.template.json",
                globalServicePolicy = $@"/{baseFileName}globalServicePolicy.template.json",
                loggers = $@"/{baseFileName}loggers.template.json",
                namedValues = $@"/{baseFileName}namedValues.template.json",
                tags = $@"/{baseFileName}tags.template.json",
                products = $@"/{baseFileName}products.template.json",
                productAPIs = $@"/{baseFileName}productAPIs.template.json",
                apiTags = $@"/{baseFileName}apiTags.template.json",
                parameters = $@"/{baseFileName}parameters.json",
                linkedMaster = $@"/{baseFileName}master.template.json",
                apis = "/Apis",
                splitAPIs = "/SplitAPIs",
                versionSetMasterFolder = "/VersionSetMasterFolder",
                revisionMasterFolder = "/RevisionMasterFolder",
                groupAPIsMasterFolder = "/MultipleApisMasterFolder",
                baseFileName = baseFileName
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
            return singleAPIName == null ? $@"/{baseFileName}apis.template.json" : $@"/{baseFileName}{singleAPIName}-api.template.json";
        }

        public static string GenerateOriginalAPIName(string apiName)
        {
            // in case the api name has been appended with ;rev={revisionNumber}, take only the api name initially provided by the user in the creator config
            string originalName = apiName.Split(";")[0];
            return originalName;
        }
    }
}
