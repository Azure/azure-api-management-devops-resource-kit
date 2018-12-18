using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
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
        public APITemplateAuthenticationSettings ConvertYAMLFileToAuthenticationSettings(string yamlFileLocation)
        {
            APITemplateAuthenticationSettings authenticationSettings = new APITemplateAuthenticationSettings();
            // Setup the input
            StreamReader streamReader = new StreamReader(yamlFileLocation);
            // Load the stream
            YamlStream yaml = new YamlStream();
            yaml.Load(streamReader);
            // Examine the stream
            YamlMappingNode mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            foreach (var entry in mapping.Children)
            {
                string key = ((YamlScalarNode)entry.Key).Value;
                if (key == "authenticationSettings")
                {
                    YamlMappingNode node = (YamlMappingNode)entry.Value;
                    // find the the values from the YAML and set the corresponding properties on the version set object
                    authenticationSettings.subscriptionKeyRequired = (string)node.Children.First(child => (string)child.Key == "subscriptionKeyRequired").Value == "true" ? true : false;
                    authenticationSettings.oAuth2 = new APITemplateOAuth2();
                    authenticationSettings.openid = new APITemplateOpenID();
                    foreach (var child in node.Children)
                    {
                        string childKey = ((YamlScalarNode)child.Key).Value;
                        if (childKey == "oAuth2")
                        {
                            YamlMappingNode childNode = (YamlMappingNode)child.Value;
                            // find the the values from the YAML and set the corresponding properties on the version set object
                            authenticationSettings.oAuth2.authorizationServerId = (string)childNode.Children.First(c => (string)c.Key == "authorizationServerId").Value;
                            authenticationSettings.oAuth2.scope = (string)childNode.Children.First(c => (string)c.Key == "scope").Value;
                        } else if (childKey == "openid")
                        {
                            YamlMappingNode childNode = (YamlMappingNode)child.Value;
                            // find the the values from the YAML and set the corresponding properties on the version set object
                            authenticationSettings.openid.openidProviderId = (string)childNode.Children.First(c => (string)c.Key == "openidProviderId").Value;
                            List<string> methods = new List<string>();
                            foreach (var subChild in childNode.Children)
                            {
                                string subkey = ((YamlScalarNode)subChild.Key).Value;
                                if(subkey == "bearerTokenSendingMethods")
                                {
                                    YamlSequenceNode subChildNode = (YamlSequenceNode)subChild.Value;
                                    foreach(YamlScalarNode deepChild in subChildNode.Children)
                                    {
                                        methods.Add(deepChild.Value);
                                    }
                                }
                            }
                            authenticationSettings.openid.bearerTokenSendingMethods = methods.ToArray();
                        }
                    }
                }
            }
            return authenticationSettings;
        }
        public APITemplateVersionSet ConvertYAMLFileToAPIVersionSet(string yamlFileLocation)
        {
            APITemplateVersionSet apiVersionSet = new APITemplateVersionSet();
            // Setup the input
            StreamReader streamReader = new StreamReader(yamlFileLocation);
            // Load the stream
            YamlStream yaml = new YamlStream();
            yaml.Load(streamReader);
            // Examine the stream
            YamlMappingNode mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            foreach (var entry in mapping.Children)
            {
                string key = ((YamlScalarNode)entry.Key).Value;
                if (key == "apiVersionSet")
                {
                    YamlMappingNode node = (YamlMappingNode)entry.Value;
                    // find the the values from the YAML and set the corresponding properties on the version set object
                    apiVersionSet.id = (string)node.Children.First(child => (string)child.Key == "id").Value;
                    apiVersionSet.description = (string)node.Children.First(child => (string)child.Key == "description").Value;
                    apiVersionSet.versionHeaderName = (string)node.Children.First(child => (string)child.Key == "versionHeaderName").Value;
                    apiVersionSet.versioningScheme = (string)node.Children.First(child => (string)child.Key == "versioningScheme").Value;
                    apiVersionSet.versionQueryName = (string)node.Children.First(child => (string)child.Key == "versionQueryName").Value;
                }
            }
            return apiVersionSet;
        }

        public Exception AttemptAPIVersionSetConversion(CLICreatorArguments cliArguments)
        {
            try
            {
                APITemplateVersionSet versionSet = ConvertYAMLFileToAPIVersionSet(cliArguments.apiVersionSetFile);
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public Exception AttemptAuthenticationSettingsConversion(CLICreatorArguments cliArguments)
        {
            try
            {
                APITemplateAuthenticationSettings authenticationSettings = ConvertYAMLFileToAuthenticationSettings(cliArguments.authenticationSettingsFile);
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}
