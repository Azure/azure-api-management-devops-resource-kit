using Newtonsoft.Json;
using System.IO;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    public class FileWriter
    {
        public void WriteJSONToFile(object template, string location)
        {
            // writes json object to provided location
            string jsonString = JsonConvert.SerializeObject(template,
                            Formatting.Indented,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
            File.WriteAllText(location, jsonString);
        }

        public CreatorFileNames GenerateCreatorLinkedFileNames(CreatorConfig creatorConfig)
        {
            string apiName = creatorConfig.api.name;
            string versionSetName = creatorConfig.apiVersionSet.displayName;
            // generate useable object with file names for consistency throughout project
            return new CreatorFileNames()
            {
                apiVersionSet = $@"/{versionSetName}.template.json",
                initialAPI = $@"/{apiName}-initial.api.template.json",
                subsequentAPI = $@"/{apiName}-subsequent.api.template.json",
                linkedMaster = $@"/{apiName}.master.template.json",
                unlinkedMasterOne = $@"/{apiName}.master1.template.json",
                unlinkedMasterTwo = $@"/{apiName}.master2.template.json",
                masterParameters = $@"/{apiName}.master.parameters.json",
            };
        }
    }
}
