using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apimtemplate.Extractor.Utilities;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class APITagExtractor : EntityExtractor
    {
        private FileWriter fileWriter;

        public APITagExtractor(FileWriter fileWriter)
        {
            this.fileWriter = fileWriter;
        }

        private async Task<string[]> GetAllOperationNames(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            JObject oOperations = new JObject();
            int numOfOps = 0;
            List<string> operationNames = new List<string>();
            do
            {
                (string azToken, string azSubId) = await auth.GetAccessToken();

                string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations?$skip={5}&api-version={6}",
                   baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, numOfOps, GlobalConstants.APIVersion);
                numOfOps += GlobalConstants.NumOfRecords;

                string operations = await CallApiManagementAsync(azToken, requestUrl);

                oOperations = JObject.Parse(operations);

                foreach (var item in oOperations["value"])
                {
                    operationNames.Add(((JValue)item["name"]).Value.ToString());
                }
            }
            while (oOperations["nextLink"] != null);
            return operationNames.ToArray();
        }

        public async Task<string> GetAPIOperationDetailsAsync(string ApiManagementName, string ResourceGroupName, string ApiName, string OperationName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations/{5}?api-version={6}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, OperationName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetOperationTagsAsync(string ApiManagementName, string ResourceGroupName, string ApiName, string OperationId)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations/{5}/tags?api-version={6}&format=rawxml",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, OperationId, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetAPIDetailsAsync(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<JToken[]> GetAllAPIObjsAsync(string ApiManagementName, string ResourceGroupName)
        {
            JObject oApi = new JObject();
            int numOfApis = 0;
            List<JToken> apiObjs = new List<JToken>();
            do
            {
                (string azToken, string azSubId) = await auth.GetAccessToken();

                string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis?$skip={4}&api-version={5}",
                baseUrl, azSubId, ResourceGroupName, ApiManagementName, numOfApis, GlobalConstants.APIVersion);
                numOfApis += GlobalConstants.NumOfRecords;

                string apis = await CallApiManagementAsync(azToken, requestUrl);

                oApi = JObject.Parse(apis);

                foreach (var item in oApi["value"])
                {
                    apiObjs.Add(item);
                }
            }
            while (oApi["nextLink"] != null);
            return apiObjs.ToArray();
        }

        public async Task<string> GetAPITagsAsync(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/tags?api-version={5}",
                baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<List<TemplateResource>> GenerateSingleAPITagResourceAsync(string apiName, Extractor exc, string[] dependsOn)
        {
            List<TemplateResource> templateResources = new List<TemplateResource>();
            string apimname = exc.sourceApimName, resourceGroup = exc.resourceGroup, fileFolder = exc.fileFolder, policyXMLBaseUrl = exc.policyXMLBaseUrl, policyXMLSasToken = exc.policyXMLSasToken;

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

        public async Task<Template> GenerateAPITagsARMTemplateAsync(string singleApiName, List<string> multipleApiNames, Extractor exc)
        {
            // initialize arm template
            Template armTemplate = GenerateEmptyApiTemplateWithParameters(exc);
            List<TemplateResource> templateResources = new List<TemplateResource>();
            // when extract single API
            if (singleApiName != null)
            {
                // check if this api exist
                try
                {
                    string apiDetails = await GetAPIDetailsAsync(exc.sourceApimName, exc.resourceGroup, singleApiName);
                    Console.WriteLine("{0} API found ...", singleApiName);
                    templateResources.AddRange(await GenerateSingleAPITagResourceAsync(singleApiName, exc, new string[] {}));
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

                string[] dependsOn = new string[] {};
                foreach (string apiName in multipleApiNames)
                {
                    templateResources.AddRange(await GenerateSingleAPITagResourceAsync(apiName, exc, dependsOn));
                    
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
            // when extract all APIs and generate one master template
            else
            {
                JToken[] oApis = await GetAllAPIObjsAsync(exc.sourceApimName, exc.resourceGroup);
                Console.WriteLine("{0} APIs found ...", (oApis.Count().ToString()));

                string[] dependsOn = new string[] {};
                foreach (JToken oApi in oApis)
                {
                    string apiName = ((JValue)oApi["name"]).Value.ToString();
                    templateResources.AddRange(await GenerateSingleAPITagResourceAsync(apiName, exc, dependsOn));
                    
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
            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}