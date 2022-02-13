using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apimtemplate.Common.Constants;
using apimtemplate.Common.TemplateModels;
using apimtemplate.Common.Templates.Abstractions;
using apimtemplate.Extractor.EntityExtractors.Abstractions;
using apimtemplate.Extractor.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace apimtemplate.Extractor.EntityExtractors
{
    public class ApiTagExtractor : ApiExtractor, IApiTagExtractor
    {
        public async Task<List<TemplateResource>> GenerateSingleAPITagResourceAsync(string apiName, ExtractorParameters extractorParameters, string[] dependsOn)
        {
            List<TemplateResource> templateResources = new List<TemplateResource>();
            string apimname = extractorParameters.sourceApimName, resourceGroup = extractorParameters.resourceGroup, fileFolder = extractorParameters.fileFolder, policyXMLBaseUrl = extractorParameters.policyXMLBaseUrl, policyXMLSasToken = extractorParameters.policyXMLSasToken;

            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting tags from {0} API:", apiName);

            string[] dependencyChain = dependsOn;

            #region Operations

            // pull api operations for service
            string[] operationNames = await GetAllOperationNames(apimname, resourceGroup, apiName);

            foreach (string operationName in operationNames)
            {
                string operationDetails = await GetAPIOperationDetailsAsync(apimname, resourceGroup, apiName, operationName);

                Console.WriteLine("'{0}' Operation found", operationName);

                // convert returned operation to template resource class
                OperationTemplateResource operationResource = JsonConvert.DeserializeObject<OperationTemplateResource>(operationDetails);
                string operationResourceName = operationResource.name;

                // add tags associated with the operation to template 
                try
                {
                    // pull tags associated with the operation
                    string apiOperationTags = await GetOperationTagsAsync(apimname, resourceGroup, apiName, operationName);
                    JObject oApiOperationTags = JObject.Parse(apiOperationTags);

                    foreach (var tag in oApiOperationTags["value"])
                    {
                        string apiOperationTagName = ((JValue)tag["name"]).Value.ToString();
                        Console.WriteLine(" - '{0}' Tag association found for {1} operation", apiOperationTagName, operationResourceName);

                        // convert operation tag association to template resource class
                        TagTemplateResource operationTagResource = JsonConvert.DeserializeObject<TagTemplateResource>(tag.ToString());
                        operationTagResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{operationResourceName}/{apiOperationTagName}')]";
                        operationTagResource.apiVersion = GlobalConstants.APIVersion;
                        operationTagResource.scale = null;
                        operationTagResource.dependsOn = dependencyChain;
                        templateResources.Add(operationTagResource);
                        dependencyChain = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis/operations/tags', parameters('{ParameterNames.ApimServiceName}'), '{apiName}', '{operationResourceName}', '{apiOperationTagName}')]" };
                    }
                }
                catch (Exception) { }
            }
            #endregion

            #region Tags
            // add tags associated with the api to template 
            try
            {
                // pull tags associated with the api
                string apiTags = await GetAPITagsAsync(apimname, resourceGroup, apiName);
                JObject oApiTags = JObject.Parse(apiTags);

                foreach (var tag in oApiTags["value"])
                {
                    string apiTagName = ((JValue)tag["name"]).Value.ToString();
                    Console.WriteLine("'{0}' Tag association found", apiTagName);

                    // convert associations between api and tags to template resource class
                    TagTemplateResource apiTagResource = JsonConvert.DeserializeObject<TagTemplateResource>(tag.ToString());
                    apiTagResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{apiTagName}')]";
                    apiTagResource.apiVersion = GlobalConstants.APIVersion;
                    apiTagResource.scale = null;
                    apiTagResource.dependsOn = dependencyChain;
                    templateResources.Add(apiTagResource);
                    dependencyChain = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis/tags', parameters('{ParameterNames.ApimServiceName}'), '{apiName}', '{apiTagName}')]" };
                }
            }
            catch (Exception) { }
            #endregion

            return templateResources;
        }

        public async Task<Template> GenerateAPITagsARMTemplateAsync(string singleApiName, List<string> multipleApiNames, ExtractorParameters extractorParameters)
        {
            // initialize arm template
            Template armTemplate = GenerateEmptyPropertyTemplateWithParameters();
            List<TemplateResource> templateResources = new List<TemplateResource>();
            // when extract single API
            if (singleApiName != null)
            {
                // check if this api exist
                try
                {
                    string apiDetails = await GetAPIDetailsAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup, singleApiName);
                    Console.WriteLine("{0} API found ...", singleApiName);
                    templateResources.AddRange(await GenerateSingleAPITagResourceAsync(singleApiName, extractorParameters, new string[] { }));
                }
                catch (Exception)
                {
                    throw new Exception($"{singleApiName} API not found!");
                }
            }
            // when extract multiple APIs and generate one master template
            else if (multipleApiNames != null)
            {
                Console.WriteLine("{0} APIs found ...", multipleApiNames.Count().ToString());

                string[] dependsOn = new string[] { };
                foreach (string apiName in multipleApiNames)
                {
                    templateResources.AddRange(await GenerateSingleAPITagResourceAsync(apiName, extractorParameters, dependsOn));

                    if (templateResources.Count > 0)
                    {

                        // Extract the tag name from the last resource
                        string[] lastTagName = templateResources.Last().name.Replace("')]", "").Split('/');
                        if (lastTagName.Length > 3)
                        {
                            // Operations tag
                            dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis/operations/tags', parameters('{ParameterNames.ApimServiceName}'), '{apiName}', '{lastTagName[2]}', '{lastTagName[3]}')]" };
                        }
                        else
                        {
                            // API tag
                            dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis/tags', parameters('{ParameterNames.ApimServiceName}'), '{apiName}', '{lastTagName[2]}')]" };
                        }
                    }
                }
            }
            // when extract all APIs and generate one master template
            else
            {
                JToken[] oApis = await GetAllApiObjsAsync(extractorParameters.sourceApimName, extractorParameters.resourceGroup);
                Console.WriteLine("{0} APIs found ...", oApis.Count().ToString());

                string[] dependsOn = new string[] { };
                foreach (JToken oApi in oApis)
                {
                    string apiName = ((JValue)oApi["name"]).Value.ToString();
                    templateResources.AddRange(await GenerateSingleAPITagResourceAsync(apiName, extractorParameters, dependsOn));
                    if (templateResources.Count > 0)
                    {
                        // Extract the tag name from the last resource
                        string[] lastTagName = templateResources.Last().name.Replace("')]", "").Split('/');
                        if (lastTagName.Length > 3)
                        {
                            // Operations tag
                            dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis/operations/tags', parameters('{ParameterNames.ApimServiceName}'), '{apiName}', '{lastTagName[2]}', '{lastTagName[3]}')]" };
                        }
                        else
                        {
                            // API tag
                            dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis/tags', parameters('{ParameterNames.ApimServiceName}'), '{apiName}', '{lastTagName[2]}')]" };
                        }
                    }
                }
            }
            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}