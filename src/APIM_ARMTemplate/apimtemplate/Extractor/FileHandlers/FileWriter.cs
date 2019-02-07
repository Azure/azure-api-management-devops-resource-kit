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
    }
}
