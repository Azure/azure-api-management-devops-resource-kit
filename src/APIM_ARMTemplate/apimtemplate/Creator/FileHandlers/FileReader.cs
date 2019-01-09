using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class FileReader
    {
        public async Task<CreatorConfig> ConvertConfigYAMLToCreatorConfigAsync(string configFileLocation)
        {
            Uri uriResult;
            bool isUrl = Uri.TryCreate(configFileLocation, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (isUrl)
            {
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
                } else {
                    throw new Exception("Unable to fetch remote config YAML file.");
                }
            }
            else
            {
                using (StreamReader reader = new StreamReader(configFileLocation))
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
        }

        public async Task<string> RetrieveLocationContentsAsync(string fileLocation)
        {
            Uri uriResult;
            bool isUrl = Uri.TryCreate(fileLocation, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            // return contents of supplied file
            if (!isUrl)
            {
                return File.ReadAllText(fileLocation);
            }
            else
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(uriResult);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    return json;
                }
                else
                {
                    throw new Exception($"Unable to retrieve contents from ${fileLocation}");
                }
            };
        }
    }
}
