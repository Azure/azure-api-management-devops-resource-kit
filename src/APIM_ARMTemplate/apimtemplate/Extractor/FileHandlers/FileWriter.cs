using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;
using Newtonsoft.Json;
using System.IO;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
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

        public CreatorFileNames GenerateCreatorFileNames()
        {
            // generate useable object with file names for consistency throughout project
            return new CreatorFileNames()
            {
                apiVersionSet = $@"/versionset.template.json",
                initialAPI = $@"/initialAPI.template.json",
                subsequentAPI = $@"/subsequentAPI.template.json",
                master = @"/master.template.json"
            };
        }
    }
}
