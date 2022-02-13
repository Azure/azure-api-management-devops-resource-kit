using Newtonsoft.Json;
using System.IO;

namespace apimtemplate.Common.FileHandlers
{
    public static class FileWriter
    {
        public static void WriteJSONToFile(object template, string location)
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

        public static void WriteXMLToFile(string xmlContent, string location)
        {
            // writes xml content to provided location
            File.WriteAllText(location, xmlContent);
        }

        public static void CreateFolderIfNotExists(string folderLocation)
        {
            // creates directory if it does not already exist
            Directory.CreateDirectory(folderLocation);
        }
    }
}
