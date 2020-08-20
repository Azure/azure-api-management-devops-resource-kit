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

        public async Task<string> GetOperationPolicyAsync(string ApiManagementName, string ResourceGroupName, string ApiName, string OperationId)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations/{5}/policies/policy?api-version={6}&format=rawxml",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, OperationId, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetOperationTagsAsync(string ApiManagementName, string ResourceGroupName, string ApiName, string OperationId)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations/{5}/tags?api-version={6}&format=rawxml",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, OperationId, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetAPIServiceUrl(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, GlobalConstants.APIVersion);

            string apiDetails = await CallApiManagementAsync(azToken, requestUrl);
            JObject oApiDetails = JObject.Parse(apiDetails);
            APITemplateResource apiResource = JsonConvert.DeserializeObject<APITemplateResource>(apiDetails);
            return apiResource.properties.serviceUrl;
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

        public async Task<List<string>> GetAllAPINamesAsync(string ApiManagementName, string ResourceGroupName)
        {
            JToken[] oApis = await GetAllAPIObjsAsync(ApiManagementName, ResourceGroupName);
            List<string> apiNames = new List<string>();

            foreach (JToken curApi in oApis)
            {
                string apiName = ((JValue)curApi["name"]).Value.ToString();
                apiNames.Add(apiName);
            }
            return apiNames;
        }

        public async Task<string> GetAPIChangeLogAsync(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/releases?api-version={5}",
                baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetAPIRevisionsAsync(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();
            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/revisions?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetAPIPolicyAsync(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/policies/policy?api-version={5}&format=rawxml",
                baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetAPITagsAsync(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/tags?api-version={5}",
                baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }
        public async Task<string> GetAPIDiagnosticsAsync(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/diagnostics?api-version={5}",
                baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetAPIProductsAsync(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/products?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetAPISchemasAsync(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/schemas?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetAPISchemaDetailsAsync(string ApiManagementName, string ResourceGroupName, string ApiName, string schemaName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/schemas/{5}?api-version={6}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, schemaName, GlobalConstants.APIVersion);

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

        // this function generate apiTemplate for single api with all its revisions
        public async Task<Template> GenerateAPITagRevisionTemplateAsync(string currentRevision, List<string> revList, string apiName, Extractor exc, string[] dependsOn)
        {
            // generate apiTemplate
            Template armTemplate = GenerateEmptyTemplateWithParameters(exc.policyXMLBaseUrl, exc.policyXMLSasToken);
            List<TemplateResource> templateResources = new List<TemplateResource>();
            Console.WriteLine("{0} APIs found ...", revList.Count().ToString());

            List<TemplateResource> apiResources = await GenerateSingleAPITagResourceAsync(apiName, exc, dependsOn);
            templateResources.AddRange(apiResources);

            string lastAPITagName = null;

            foreach (string curApi in revList)
            {
                // add current API revision resource to template
                apiResources = await GenerateSingleAPITagResourceAsync(curApi, exc, (lastAPITagName != null ? new string[] { lastAPITagName } : dependsOn));
                templateResources.AddRange(apiResources);

                // Extract the tag name from the last resource
                string[] lastTagName = apiResources.Last().name.Replace("')]", "").Split('/');
                if (lastTagName.Length > 3)
                {
                    // Operations tag
                    lastAPITagName = $"[resourceId('Microsoft.ApiManagement/service/apis/operations/tags', parameters('{ParameterNames.ApimServiceName}'), '{apiName}', '{lastTagName[2]}', '{lastTagName[3]}')]";
                }
                else
                {
                    // API tag
                    lastAPITagName = $"[resourceId('Microsoft.ApiManagement/service/apis/tags', parameters('{ParameterNames.ApimServiceName}'), '{apiName}', '{lastTagName[2]}')]";
                }
            }

            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
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

        private static void ArmEscapeSampleValueIfNecessary(OperationTemplateRepresentation operationTemplateRepresentation)
        {
            if (!string.IsNullOrWhiteSpace(operationTemplateRepresentation.sample) && operationTemplateRepresentation.contentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true && operationTemplateRepresentation.sample.TryParseJson(out JToken sampleAsJToken) && sampleAsJToken.Type == JTokenType.Array)
            {
                operationTemplateRepresentation.sample = "[" + operationTemplateRepresentation.sample;
            }
        }

        private static void AddSchemaDependencyToOperationIfNecessary(string oApiName, List<string> operationDependsOn, OperationTemplateRepresentation operationTemplateRepresentation)
        {
            if (operationTemplateRepresentation.schemaId != null)
            {
                string dependsOn = $"[resourceId('Microsoft.ApiManagement/service/apis/schemas', parameters('{ParameterNames.ApimServiceName}'), '{oApiName}', '{operationTemplateRepresentation.schemaId}')]";
                // add value to list if schema has not already been added
                if (!operationDependsOn.Exists(o => o == dependsOn))
                {
                    operationDependsOn.Add(dependsOn);
                }
            }
        }

        private static bool CheckAPIExist(string singleApiName, JObject oApi)
        {
            for (int i = 0; i < ((JContainer)oApi["value"]).Count; i++)
            {
                if (((JValue)oApi["value"][i]["name"]).Value.ToString().Equals(singleApiName))
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<List<TemplateResource>> GenerateSchemasARMTemplate(string apimServiceName, string apiName, string resourceGroup, string fileFolder)
        {
            List<TemplateResource> templateResources = new List<TemplateResource>();

            // pull all schemas from service
            string schemas = await GetAPISchemasAsync(apimServiceName, resourceGroup, apiName);
            JObject oSchemas = JObject.Parse(schemas);

            foreach (var item in oSchemas["value"])
            {
                string schemaName = ((JValue)item["name"]).Value.ToString();
                Console.WriteLine("'{0}' Operation schema found", schemaName);

                string schemaDetails = await GetAPISchemaDetailsAsync(apimServiceName, resourceGroup, apiName, schemaName);

                // pull returned schema and convert to template resource
                RESTReturnedSchemaTemplate restReturnedSchemaTemplate = JsonConvert.DeserializeObject<RESTReturnedSchemaTemplate>(schemaDetails);
                SchemaTemplateResource schemaDetailsResource = JsonConvert.DeserializeObject<SchemaTemplateResource>(schemaDetails);
                schemaDetailsResource.properties.document.value = GetSchemaValueBasedOnContentType(restReturnedSchemaTemplate.properties);
                schemaDetailsResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{schemaName}')]";
                schemaDetailsResource.apiVersion = GlobalConstants.APIVersion;
                schemaDetailsResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiName}')]" };

                templateResources.Add(schemaDetailsResource);

            }
            return templateResources;
        }

        private string GetSchemaValueBasedOnContentType(RESTReturnedSchemaTemplateProperties schemaTemplateProperties)
        {
            var contentType = schemaTemplateProperties.contentType.ToLowerInvariant();
            if (contentType.Equals("application/vnd.oai.openapi.components+json"))
            {
                // for OpenAPI "value" is not used, but "components" which is resolved during json deserialization
                return null;
            }

            if (!(schemaTemplateProperties.document is JToken))
            {
                return JsonConvert.SerializeObject(schemaTemplateProperties.document);
            }

            var schemaJson = schemaTemplateProperties.document as JToken;

            switch (contentType)
            {
                case "application/vnd.ms-azure-apim.swagger.definitions+json":
                    if (schemaJson["definitions"] != null && schemaJson.Count() == 1)
                    {
                        schemaJson = schemaJson["definitions"];
                    }
                    break;
                case "application/vnd.ms-azure-apim.xsd+xml":
                    if (schemaJson["value"] != null && schemaJson.Count() == 1)
                    {
                        return schemaJson["value"].ToString();
                    }
                    break;
            }

            return JsonConvert.SerializeObject(schemaJson);
        }
    }
}