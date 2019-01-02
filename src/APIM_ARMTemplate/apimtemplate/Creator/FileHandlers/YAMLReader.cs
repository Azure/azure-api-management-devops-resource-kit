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
    public class YAMLReader
    {
        public CreatorConfig ConvertConfigYAMLToCreatorConfig(string configFileLocation)
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

        public async Task<string> RetrieveLocationContents(string fileLocation)
        {
            Uri uriResult;
            bool isUrl = Uri.TryCreate(fileLocation, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            // return contents of supplied Open API Spec file
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
                    return "";
                }
            };
        }
    }
}
