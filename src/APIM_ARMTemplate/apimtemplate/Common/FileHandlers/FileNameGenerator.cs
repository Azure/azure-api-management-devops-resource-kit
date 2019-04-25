using Newtonsoft.Json;
using System.IO;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    public class FileNameGenerator
    {
        public CreatorFileNames GenerateCreatorLinkedFileNames(CreatorConfig creatorConfig)
        {
            // generate useable object with file names for consistency throughout project
            return new CreatorFileNames()
            {
                apiVersionSets = @"/api-version-sets.template.json",
                products = @"/products.template.json",
                loggers = @"/loggers.template.json",
                linkedMaster = @"/master.template.json",
                linkedParameters = @"/master.parameters.json",
                unlinkedParameters = @"/parameters.json"
            };
        }

        public string GenerateAPIFileName(string apiName, bool isInitialAPI)
        {
            return isInitialAPI == true ? $@"/{apiName}-initial.api.template.json" : $@"/{apiName}-subsequent.api.template.json";
        }
    }
}
