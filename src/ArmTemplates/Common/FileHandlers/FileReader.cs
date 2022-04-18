// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Configurations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers
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
                        CreatorConfig yamlObject = jsonText.Deserialize<CreatorConfig>();
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
                    CreatorConfig yamlObject = jsonText.Deserialize<CreatorConfig>();
                    return yamlObject;
                }
            }
        }

        public ExtractorConsoleAppConfiguration ConvertConfigJsonToExtractorConfig(string extractorJsonPath)
        {
            if (string.IsNullOrWhiteSpace(extractorJsonPath) || !File.Exists(extractorJsonPath))
            {
                throw new FileNotFoundException($"You have to specify an existing file, you specified: '{extractorJsonPath}'");
            }

            using (StreamReader r = new StreamReader(extractorJsonPath))
            {
                string extractorJson = r.ReadToEnd();
                ExtractorConsoleAppConfiguration extractorConfig = extractorJson.Deserialize<ExtractorConsoleAppConfiguration>();
                return extractorConfig;
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
                return this.RetrieveLocalFileContents(fileLocation);
            }
        }

        public bool IsJSON(string fileContents)
        {
            try
            {
                object deserializedFileContents = fileContents.Deserialize<object>();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
