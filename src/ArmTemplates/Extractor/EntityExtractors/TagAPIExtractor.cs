using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class TagApiExtractor : ApiExtractor, ITagApiExtractor
    {
        public async Task<List<TemplateResource>> GenerateSingleAPITagResourceAsync(string apiName, ExtractorParameters extractorParameters, string[] dependsOn)
        {
            List<TemplateResource> templateResources = new List<TemplateResource>();
            string apimname = extractorParameters.SourceApimName, resourceGroup = extractorParameters.ResourceGroup, fileFolder = extractorParameters.FilesGenerationRootDirectory, policyXMLBaseUrl = extractorParameters.PolicyXMLBaseUrl, policyXMLSasToken = extractorParameters.PolicyXMLSasToken;

            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting tags from {0} API:", apiName);

            string[] dependencyChain = dependsOn;

            #region Operations

            // pull api operations for service
            string[] operationNames = await this.GetAllOperationNames(apimname, resourceGroup, apiName);

            foreach (string operationName in operationNames)
            {
                string operationDetails = await this.GetAPIOperationDetailsAsync(apimname, resourceGroup, apiName, operationName);

                Console.WriteLine("'{0}' Operation found", operationName);

                // convert returned operation to template resource class
                OperationTemplateResource operationResource = JsonConvert.DeserializeObject<OperationTemplateResource>(operationDetails);
                string operationResourceName = operationResource.Name;

                // add tags associated with the operation to template 
                try
                {
                    // pull tags associated with the operation
                    string apiOperationTags = await this.GetOperationTagsAsync(apimname, resourceGroup, apiName, operationName);
                    JObject oApiOperationTags = JObject.Parse(apiOperationTags);

                    foreach (var tag in oApiOperationTags["value"])
                    {
                        string apiOperationTagName = ((JValue)tag["name"]).Value.ToString();
                        Console.WriteLine(" - '{0}' Tag association found for {1} operation", apiOperationTagName, operationResourceName);

                        // convert operation tag association to template resource class
                        TagTemplateResource operationTagResource = JsonConvert.DeserializeObject<TagTemplateResource>(tag.ToString());
                        operationTagResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{operationResourceName}/{apiOperationTagName}')]";
                        operationTagResource.ApiVersion = GlobalConstants.ApiVersion;
                        operationTagResource.Scale = null;
                        operationTagResource.DependsOn = dependencyChain;
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
                string apiTags = await this.GetAPITagsAsync(apimname, resourceGroup, apiName);
                JObject oApiTags = JObject.Parse(apiTags);

                foreach (var tag in oApiTags["value"])
                {
                    string apiTagName = ((JValue)tag["name"]).Value.ToString();
                    Console.WriteLine("'{0}' Tag association found", apiTagName);

                    // convert associations between api and tags to template resource class
                    TagTemplateResource apiTagResource = JsonConvert.DeserializeObject<TagTemplateResource>(tag.ToString());
                    apiTagResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{apiTagName}')]";
                    apiTagResource.ApiVersion = GlobalConstants.ApiVersion;
                    apiTagResource.Scale = null;
                    apiTagResource.DependsOn = dependencyChain;
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
            Template armTemplate = this.GenerateEmptyPropertyTemplateWithParameters();
            List<TemplateResource> templateResources = new List<TemplateResource>();
            // when extract single API
            if (singleApiName != null)
            {
                // check if this api exist
                try
                {
                    string apiDetails = await this.GetAPIDetailsAsync(extractorParameters.SourceApimName, extractorParameters.ResourceGroup, singleApiName);
                    Console.WriteLine("{0} API found ...", singleApiName);
                    templateResources.AddRange(await this.GenerateSingleAPITagResourceAsync(singleApiName, extractorParameters, new string[] { }));
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
                    templateResources.AddRange(await this.GenerateSingleAPITagResourceAsync(apiName, extractorParameters, dependsOn));

                    if (templateResources.Count > 0)
                    {

                        // Extract the tag name from the last resource
                        string[] lastTagName = templateResources.Last().Name.Replace("')]", "").Split('/');
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
                JToken[] oApis = await this.GetAllApiObjsAsync(extractorParameters.SourceApimName, extractorParameters.ResourceGroup);
                Console.WriteLine("{0} APIs found ...", oApis.Count().ToString());

                string[] dependsOn = new string[] { };
                foreach (JToken oApi in oApis)
                {
                    string apiName = ((JValue)oApi["name"]).Value.ToString();
                    templateResources.AddRange(await this.GenerateSingleAPITagResourceAsync(apiName, extractorParameters, dependsOn));
                    if (templateResources.Count > 0)
                    {
                        // Extract the tag name from the last resource
                        string[] lastTagName = templateResources.Last().Name.Replace("')]", "").Split('/');
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
            armTemplate.Resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}