﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class FileReader
    {
        public async Task<CreatorConfig> ConvertConfigYAMLToCreatorConfigAsync(string configFileLocation)
        {
            // determine whether file location is local file path or remote url and convert appropriately
            Uri uriResult;
            bool isUrl = Uri.TryCreate(configFileLocation, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
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
                } else {
                    throw new Exception("Unable to fetch remote config YAML file.");
                }
            }
            else
            {
                using (StreamReader reader = new StreamReader(configFileLocation))
                {
                    // deserialize provided file contents into yaml
                    Deserializer deserializer = new Deserializer();
                    object deserializedYaml = deserializer.Deserialize(reader);
                    JsonSerializer jsonSerializer = new JsonSerializer();
                    StringWriter writer = new StringWriter();
                    // serialize json from yaml object
                    jsonSerializer.Serialize(writer, deserializedYaml);
                    string jsonText = writer.ToString();
                    // deserialize CreatorConfig from json string
                    CreatorConfig yamlObject = JsonConvert.DeserializeObject<CreatorConfig>(jsonText);
                    return yamlObject;
                }
            }
        }

        public string RetrieveLocalFileContents(string fileLocation)
        {
            return File.ReadAllText(fileLocation);
        }
    }
}
