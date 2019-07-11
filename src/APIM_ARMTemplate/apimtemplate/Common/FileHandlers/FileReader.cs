using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;
using System.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    public class FileReader
    {
        public async Task<CreatorConfig> ConvertConfigYAMLToCreatorConfigAsync(string configFileLocation)
        {
            // substitute Environment Variables
            var finalLocation = Environment.ExpandEnvironmentVariables(configFileLocation);


            // determine whether file location is local file path or remote url and convert appropriately
            Uri uriResult;
            bool isUrl = Uri.TryCreate(finalLocation, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (isUrl)
            {
                // make a request to the provided url and convert the response's content
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(uriResult);
                if (response.IsSuccessStatusCode)
                {
                    Stream stream = await response.Content.ReadAsStreamAsync();
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        Deserializer deserializer = new Deserializer();
                        object deserializedYaml = deserializer.Deserialize(reader);
                        JsonSerializer jsonSerializer = new JsonSerializer();
                        StringWriter writer = new StringWriter();
                        jsonSerializer.Serialize(writer, deserializedYaml);
                        string jsonText = writer.ToString();
                        CreatorConfig yamlObject = JsonConvert.DeserializeObject<CreatorConfig>(jsonText);
                        return yamlObject;
                    }
                }
                else
                {
                    throw new Exception("Unable to fetch remote config YAML file.");
                }
            }
            else
            {
                using (StreamReader reader = new StreamReader(finalLocation))
                {
                    string originalYaml = await reader.ReadToEndAsync();

                    var yamlSubstituted = Environment.ExpandEnvironmentVariables(originalYaml);

                    // deserialize provided file contents into yaml object
                    Deserializer deserializer = new Deserializer();
                    CreatorConfig yamlObject = deserializer.Deserialize<CreatorConfig>(yamlSubstituted);

                    return yamlObject;
                }
            }
        }

        public string RetrieveLocalFileContents(string fileLocation)
        {
            return File.ReadAllText(fileLocation);
        }

        public async Task<string> RetrieveFileContentsAsync(string fileLocation)
        {
            // determine whether file location is local file path or remote url and convert appropriately
            Uri uriResult;
            bool isUrl = Uri.TryCreate(fileLocation, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (isUrl)
            {
                // make a request to the provided url and convert the response's content
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(uriResult);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    return content;
                }
                else
                {
                    throw new Exception($"Unable to fetch remote file - {fileLocation}");
                }
            }
            else
            {
                return RetrieveLocalFileContents(fileLocation);
            }
        }

        public bool isJSON(string fileContents)
        {
            try
            {
                object deserializedFileContents = JsonConvert.DeserializeObject<object>(fileContents);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
