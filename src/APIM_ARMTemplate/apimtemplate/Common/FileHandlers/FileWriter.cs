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
            // generate useable object with file names for consistency throughout project
            return new CreatorFileNames()
            {
                apiVersionSet = @"/version-sets.template.json",
                linkedMaster = @"/master.template.json",
                masterParameters = @"/master.parameters.json",
            };
        }
    }
}
