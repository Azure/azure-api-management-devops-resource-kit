using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities;
using Microsoft.Azure.Management.Monitor.Fluent.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class ApiExtractor : EntityExtractorBase, IApiExtractor
    {
        public async Task<string[]> GetAllOperationNames(string apiManagementName, string resourceGroupName, string apiName)
        {
            var oOperations = new JObject();
            int numOfOps = 0;
            List<string> operationNames = new List<string>();
            do
            {
                (string azToken, string azSubId) = await this.Auth.GetAccessToken();

                string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations?$skip={5}&api-version={6}",
                   BaseUrl, azSubId, resourceGroupName, apiManagementName, apiName, numOfOps, GlobalConstants.ApiVersion);
                numOfOps += GlobalConstants.NumOfRecords;

                string operations = await this.CallApiManagementAsync(azToken, requestUrl);

                oOperations = JObject.Parse(operations);

                foreach (var item in oOperations["value"])
                {
                    operationNames.Add(((JValue)item["name"]).Value.ToString());
                }
            }
            while (oOperations["nextLink"] != null);
            return operationNames.ToArray();
        }

        public async Task<string> GetAPIOperationDetailsAsync(string apiManagementName, string resourceGroupName, string apiName, string operationName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations/{5}?api-version={6}",
               BaseUrl, azSubId, resourceGroupName, apiManagementName, apiName, operationName, GlobalConstants.ApiVersion);

            return await this.CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetOperationPolicyAsync(string apiManagementName, string resourceGroupName, string apiName, string operationId)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations/{5}/policies/policy?api-version={6}&format=rawxml",
               BaseUrl, azSubId, resourceGroupName, apiManagementName, apiName, operationId, GlobalConstants.ApiVersion);

            return await this.CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetOperationTagsAsync(string apiManagementName, string resourceGroupName, string apiName, string operationId)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations/{5}/tags?api-version={6}&format=rawxml",
               BaseUrl, azSubId, resourceGroupName, apiManagementName, apiName, operationId, GlobalConstants.ApiVersion);

            return await this.CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetAPIServiceUrl(string apiManagementName, string resourceGroupName, string apiName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}?api-version={5}",
               BaseUrl, azSubId, resourceGroupName, apiManagementName, apiName, GlobalConstants.ApiVersion);

            string apiDetails = await this.CallApiManagementAsync(azToken, requestUrl);
            JObject oApiDetails = JObject.Parse(apiDetails);
            APITemplateResource apiResource = JsonConvert.DeserializeObject<APITemplateResource>(apiDetails);
            return apiResource.Properties.ServiceUrl;
        }

        public async Task<string> GetAPIDetailsAsync(string apiManagementName, string resourceGroupName, string apiName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}?api-version={5}",
               BaseUrl, azSubId, resourceGroupName, apiManagementName, apiName, GlobalConstants.ApiVersion);

            return await this.CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<JToken[]> GetAllApiObjsAsync(string apiManagementName, string resourceGroupName)
        {
            JObject oApi = new JObject();
            int numOfApis = 0;
            List<JToken> apiObjs = new List<JToken>();
            do
            {
                (string azToken, string azSubId) = await this.Auth.GetAccessToken();

                string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis?$skip={4}&api-version={5}",
                BaseUrl, azSubId, resourceGroupName, apiManagementName, numOfApis, GlobalConstants.ApiVersion);
                numOfApis += GlobalConstants.NumOfRecords;

                string apis = await this.CallApiManagementAsync(azToken, requestUrl);

                oApi = JObject.Parse(apis);

                foreach (var item in oApi["value"])
                {
                    apiObjs.Add(item);
                }
            }
            while (oApi["nextLink"] != null);
            return apiObjs.ToArray();
        }

        public async Task<List<string>> GetAllApiNamesAsync(string apiManagementName, string resourceGroupName)
        {
            JToken[] oApis = await this.GetAllApiObjsAsync(apiManagementName, resourceGroupName);
            List<string> apiNames = new List<string>();

            foreach (JToken curApi in oApis)
            {
                string apiName = ((JValue)curApi["name"]).Value.ToString();
                apiNames.Add(apiName);
            }
            return apiNames;
        }

        public async Task<string> GetAPIChangeLogAsync(string apiManagementName, string resourceGroupName, string apiName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/releases?api-version={5}",
                BaseUrl, azSubId, resourceGroupName, apiManagementName, apiName, GlobalConstants.ApiVersion);

            return await this.CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetAPIRevisionsAsync(string apiManagementName, string resourceGroupName, string apiName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();
            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/revisions?api-version={5}",
               BaseUrl, azSubId, resourceGroupName, apiManagementName, apiName, GlobalConstants.ApiVersion);

            return await this.CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetAPIPolicyAsync(string apiManagementName, string resourceGroupName, string apiName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/policies/policy?api-version={5}&format=rawxml",
                BaseUrl, azSubId, resourceGroupName, apiManagementName, apiName, GlobalConstants.ApiVersion);

            return await this.CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetAPITagsAsync(string apiManagementName, string resourceGroupName, string apiName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/tags?api-version={5}",
                BaseUrl, azSubId, resourceGroupName, apiManagementName, apiName, GlobalConstants.ApiVersion);

            return await this.CallApiManagementAsync(azToken, requestUrl);
        }
        public async Task<string> GetApiDiagnosticsAsync(string apiManagementName, string resourceGroupName, string apiName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/diagnostics?api-version={5}",
                BaseUrl, azSubId, resourceGroupName, apiManagementName, apiName, GlobalConstants.ApiVersion);

            return await this.CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetAPIProductsAsync(string apiManagementName, string resourceGroupName, string apiName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/products?api-version={5}",
               BaseUrl, azSubId, resourceGroupName, apiManagementName, apiName, GlobalConstants.ApiVersion);

            return await this.CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetAPISchemasAsync(string apiManagementName, string resourceGroupName, string apiName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/schemas?api-version={5}",
               BaseUrl, azSubId, resourceGroupName, apiManagementName, apiName, GlobalConstants.ApiVersion);

            return await this.CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetAPISchemaDetailsAsync(string apiManagementName, string resourceGroupName, string apiName, string schemaName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/schemas/{5}?api-version={6}",
               BaseUrl, azSubId, resourceGroupName, apiManagementName, apiName, schemaName, GlobalConstants.ApiVersion);

            return await this.CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetServiceDiagnosticsAsync(string apiManagementName, string resourceGroupName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/diagnostics?api-version={4}",
                BaseUrl, azSubId, resourceGroupName, apiManagementName, GlobalConstants.ApiVersion);

            return await this.CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<List<TemplateResource>> GenerateSingleAPIResourceAsync(ExtractorParameters extractorParameters, string apiName, string baseFilesGenerationDirectory)
        {
            List<TemplateResource> templateResources = new List<TemplateResource>();
            string apiDetails = await this.GetAPIDetailsAsync(extractorParameters.SourceApimName, extractorParameters.ResourceGroup, apiName);

            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting resources from {0} API:", apiName);

            // convert returned api to template resource class
            JObject oApiDetails = JObject.Parse(apiDetails);
            APITemplateResource apiResource = JsonConvert.DeserializeObject<APITemplateResource>(apiDetails);

            apiResource.Type = ((JValue)oApiDetails["type"]).Value.ToString();
            apiResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}')]";
            apiResource.ApiVersion = GlobalConstants.ApiVersion;
            apiResource.Scale = null;

            if (extractorParameters.ToParameterizeServiceUrl)
            {
                apiResource.Properties.ServiceUrl = $"[parameters('{ParameterNames.ServiceUrl}').{ParameterNamingHelper.GenerateValidParameterName(apiName, ParameterPrefix.Api)}]";
            }

            if (apiResource.Properties.ApiVersionSetId != null)
            {
                apiResource.DependsOn = new string[] { };

                string versionSetName = apiResource.Properties.ApiVersionSetId;
                int versionSetPosition = versionSetName.IndexOf("apiVersionSets/");

                versionSetName = versionSetName.Substring(versionSetPosition, versionSetName.Length - versionSetPosition);
                apiResource.Properties.ApiVersionSetId = $"[concat(resourceId('Microsoft.ApiManagement/service', parameters('{ParameterNames.ApimServiceName}')), '/{versionSetName}')]";
            }
            else
            {
                apiResource.DependsOn = new string[] { };
            }

            templateResources.Add(apiResource);

            templateResources.AddRange(await this.GetRelatedTemplateResourcesAsync(extractorParameters, apiName, baseFilesGenerationDirectory));

            return templateResources;
        }

        // this function generate apiTemplate for single api with all its revisions
        public async Task<Template> GenerateAPIRevisionTemplateAsync(ExtractorParameters extractorParameters, string currentRevision, List<string> revList, string baseFilesGenerationDirectory)
        {
            // generate apiTemplate
            Template armTemplate = TemplateCreator.GenerateEmptyApiTemplateWithParameters(extractorParameters);
            List<TemplateResource> templateResources = new List<TemplateResource>();
            Console.WriteLine("{0} APIs found ...", revList.Count().ToString());

            List<TemplateResource> apiResources = await this.GenerateSingleAPIResourceAsync(extractorParameters, extractorParameters.SingleApiName, baseFilesGenerationDirectory);
            templateResources.AddRange(apiResources);

            foreach (string curApi in revList)
            {
                // should add current api to dependsOn to those revisions that are not "current"
                if (curApi.Equals(currentRevision))
                {
                    // add current API revision resource to template
                    apiResources = await this.GenerateCurrentRevisionAPIResourceAsync(extractorParameters, curApi, baseFilesGenerationDirectory);
                    templateResources.AddRange(apiResources);
                }
                else
                {
                    // add other API revision resources to template
                    apiResources = await this.GenerateSingleAPIResourceAsync(extractorParameters, curApi, baseFilesGenerationDirectory);

                    // make current API a dependency to other revisions, in case destination apim doesn't have the this API 
                    TemplateResource apiResource = apiResources.FirstOrDefault(resource => resource.Type == ResourceTypeConstants.API) as TemplateResource;
                    List<TemplateResource> newResourcesList = RemoveResourceType(ResourceTypeConstants.API, apiResources);
                    List<string> dependsOn = apiResource.DependsOn.ToList();
                    dependsOn.Add($"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{extractorParameters.SingleApiName}')]");
                    apiResource.DependsOn = dependsOn.ToArray();
                    newResourcesList.Add(apiResource);

                    templateResources.AddRange(newResourcesList);
                }
            }

            armTemplate.Resources = templateResources.ToArray();
            return armTemplate;
        }

        // this function will get the current revision of this api and will remove "isCurrent" paramter
        public async Task<List<TemplateResource>> GenerateCurrentRevisionAPIResourceAsync(ExtractorParameters extractorParameters, string apiName, string baseFilesGenerationDirectory)
        {
            List<TemplateResource> templateResources = new List<TemplateResource>();
            string apimname = extractorParameters.SourceApimName, resourceGroup = extractorParameters.ResourceGroup;
            string apiDetails = await this.GetAPIDetailsAsync(apimname, resourceGroup, apiName);

            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting resources from {0} API:", apiName);

            // convert returned api to template resource class
            JObject oApiDetails = JObject.Parse(apiDetails);
            APITemplateResource apiResource = JsonConvert.DeserializeObject<APITemplateResource>(apiDetails);

            apiResource.Type = ((JValue)oApiDetails["type"]).Value.ToString();
            apiResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}')]";
            apiResource.ApiVersion = GlobalConstants.ApiVersion;
            apiResource.Scale = null;
            apiResource.Properties.IsCurrent = null;

            if (extractorParameters.ToParameterizeServiceUrl)
            {
                apiResource.Properties.ServiceUrl = $"[parameters('{ParameterNames.ServiceUrl}').{ParameterNamingHelper.GenerateValidParameterName(apiName, ParameterPrefix.Api)}]";
            }

            if (apiResource.Properties.ApiVersionSetId != null)
            {
                apiResource.DependsOn = new string[] { };

                string versionSetName = apiResource.Properties.ApiVersionSetId;
                int versionSetPosition = versionSetName.IndexOf("apiVersionSets/");

                versionSetName = versionSetName.Substring(versionSetPosition, versionSetName.Length - versionSetPosition);
                apiResource.Properties.ApiVersionSetId = $"[concat(resourceId('Microsoft.ApiManagement/service', parameters('{ParameterNames.ApimServiceName}')), '/{versionSetName}')]";
            }
            else
            {
                apiResource.DependsOn = new string[] { };
            }

            templateResources.Add(apiResource);
            templateResources.AddRange(await this.GetRelatedTemplateResourcesAsync(extractorParameters, apiName, baseFilesGenerationDirectory));

            return templateResources;
        }

        public async Task<Template> GenerateAPIsARMTemplateAsync(ExtractorParameters extractorParameters, string singleApiName, List<string> multipleApiNames, string baseFilesGenerationDirectory)
        {
            // initialize arm template
            Template armTemplate = TemplateCreator.GenerateEmptyApiTemplateWithParameters(extractorParameters);
            List<TemplateResource> templateResources = new List<TemplateResource>();
            
            // when extract single API
            if (singleApiName != null)
            {
                // check if this api exist
                try
                {
                    string apiDetails = await this.GetAPIDetailsAsync(extractorParameters.SourceApimName, extractorParameters.ResourceGroup, singleApiName);
                    Console.WriteLine("{0} API found ...", singleApiName);
                    templateResources.AddRange(await this.GenerateSingleAPIResourceAsync(extractorParameters, singleApiName, baseFilesGenerationDirectory));
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
                foreach (string apiName in multipleApiNames)
                {
                    templateResources.AddRange(await this.GenerateSingleAPIResourceAsync(extractorParameters, apiName, baseFilesGenerationDirectory));
                }
            }
            // when extract all APIs and generate one master template
            else
            {
                JToken[] oApis = await this.GetAllApiObjsAsync(extractorParameters.SourceApimName, extractorParameters.ResourceGroup);
                Console.WriteLine("{0} APIs found ...", oApis.Count().ToString());

                foreach (JToken oApi in oApis)
                {
                    string apiName = ((JValue)oApi["name"]).Value.ToString();
                    templateResources.AddRange(await this.GenerateSingleAPIResourceAsync(extractorParameters, apiName, baseFilesGenerationDirectory));
                }
            }


            //Add the All API Diagnostics settings

            templateResources.AddRange(await this.GetServiceDiagnosticsTemplateResourcesAsync(extractorParameters));


            armTemplate.Resources = templateResources.ToArray();
            return armTemplate;
        }

        static void ArmEscapeSampleValueIfNecessary(OperationTemplateRepresentation operationTemplateRepresentation)
        {

            if (!string.IsNullOrWhiteSpace(operationTemplateRepresentation.sample) &&
                ContentTypes().Contains(operationTemplateRepresentation.contentType?.ToLower()) &&
                operationTemplateRepresentation.sample.TryParseJson(out JToken sampleAsJToken) &&
                sampleAsJToken.Type == JTokenType.Array)
            {
                operationTemplateRepresentation.sample = "[" + operationTemplateRepresentation.sample;
            }
        }

        static string[] ContentTypes()
        {
            return new string[] { "application/json", "text/json", "application/*+json" };
        }

        static void AddSchemaDependencyToOperationIfNecessary(string apiName, List<string> operationDependsOn, OperationTemplateRepresentation operationTemplateRepresentation)
        {
            if (operationTemplateRepresentation.schemaId != null)
            {
                string dependsOn = $"[resourceId('Microsoft.ApiManagement/service/apis/schemas', parameters('{ParameterNames.ApimServiceName}'), '{apiName}', '{operationTemplateRepresentation.schemaId}')]";
                // add value to list if schema has not already been added
                if (!operationDependsOn.Exists(o => o == dependsOn))
                {
                    operationDependsOn.Add(dependsOn);
                }
            }
        }

        static bool CheckAPIExist(string singleApiName, JObject oApi)
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

        async Task<List<TemplateResource>> GenerateSchemasARMTemplate(string apimServiceName, string apiName, string resourceGroup)
        {
            List<TemplateResource> templateResources = new List<TemplateResource>();

            // pull all schemas from service
            string schemas = await this.GetAPISchemasAsync(apimServiceName, resourceGroup, apiName);
            JObject oSchemas = JObject.Parse(schemas);

            foreach (var item in oSchemas["value"])
            {
                string schemaName = ((JValue)item["name"]).Value.ToString();
                Console.WriteLine("'{0}' Operation schema found", schemaName);

                string schemaDetails = await this.GetAPISchemaDetailsAsync(apimServiceName, resourceGroup, apiName, schemaName);

                // pull returned schema and convert to template resource
                RESTReturnedSchemaTemplate restReturnedSchemaTemplate = JsonConvert.DeserializeObject<RESTReturnedSchemaTemplate>(schemaDetails);
                SchemaTemplateResource schemaDetailsResource = JsonConvert.DeserializeObject<SchemaTemplateResource>(schemaDetails);
                schemaDetailsResource.properties.document.value = this.GetSchemaValueBasedOnContentType(restReturnedSchemaTemplate.properties);
                schemaDetailsResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{schemaName}')]";
                schemaDetailsResource.ApiVersion = GlobalConstants.ApiVersion;
                schemaDetailsResource.DependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiName}')]" };

                templateResources.Add(schemaDetailsResource);

            }
            return templateResources;
        }

        string GetSchemaValueBasedOnContentType(RESTReturnedSchemaTemplateProperties schemaTemplateProperties)
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

        /// <summary>
        /// Adds related API Template resources like schemas, operations, products, tags etc.
        /// </summary>
        /// <param name="apiName">The name of the API.</param>
        /// <param name="extractorParameters">The extractor.</param>
        /// <returns></returns>
        async Task<IEnumerable<TemplateResource>> GetRelatedTemplateResourcesAsync(ExtractorParameters extractorParameters, string apiName, string baseFilesGenerationDirectory)
        {
            List<TemplateResource> templateResources = new List<TemplateResource>();
            var apimname = extractorParameters.SourceApimName;
            var resourceGroup = extractorParameters.ResourceGroup;
            var fileFolder = baseFilesGenerationDirectory;
            var policyXMLBaseUrl = extractorParameters.PolicyXMLBaseUrl;
            var policyXMLSasToken = extractorParameters.PolicyXMLSasToken;

            #region Schemas
            // add schema resources to api template
            List<TemplateResource> schemaResources = await this.GenerateSchemasARMTemplate(apimname, apiName, resourceGroup);
            templateResources.AddRange(schemaResources);
            #endregion

            #region Operations
            // pull api operations for service
            string[] operationNames = await this.GetAllOperationNames(apimname, resourceGroup, apiName);
            int numBatches = 0;

            // create empty array for the batch operation owners
            List<string> batchOwners = new List<string>();

            // if a batch size is specified            
            if (extractorParameters.OperationBatchSize > 0)
            {
                // store the number of batches required based on exc.operationBatchSize
                numBatches = (int)Math.Ceiling(operationNames.Length / (double)extractorParameters.OperationBatchSize);
                //Console.WriteLine ("Number of batches: {0}", numBatches);
            }


            foreach (string operationName in operationNames)
            {
                int opIndex = Array.IndexOf(operationNames, operationName);

                //add batch owners into array
                // ensure each owner is linked to the one before
                if (extractorParameters.OperationBatchSize > 0 && opIndex < numBatches)
                {
                    batchOwners.Add(operationName);
                    //Console.WriteLine("Adding operation {0} to owner list", operationName);
                }

                string operationDetails = await this.GetAPIOperationDetailsAsync(apimname, resourceGroup, apiName, operationName);

                Console.WriteLine("'{0}' Operation found", operationName);

                // convert returned operation to template resource class
                OperationTemplateResource operationResource = JsonConvert.DeserializeObject<OperationTemplateResource>(operationDetails);
                string operationResourceName = operationResource.Name;
                operationResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{operationResourceName}')]";
                operationResource.ApiVersion = GlobalConstants.ApiVersion;
                operationResource.Scale = null;

                // add operation dependencies and fix sample value if necessary
                List<string> operationDependsOn = new List<string>() { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiName}')]" };
                foreach (OperationTemplateRepresentation operationTemplateRepresentation in operationResource.properties.request.representations)
                {
                    AddSchemaDependencyToOperationIfNecessary(apiName, operationDependsOn, operationTemplateRepresentation);
                    ArmEscapeSampleValueIfNecessary(operationTemplateRepresentation);
                }

                foreach (OperationsTemplateResponse operationTemplateResponse in operationResource.properties.responses)
                {
                    foreach (OperationTemplateRepresentation operationTemplateRepresentation in operationTemplateResponse.representations)
                    {
                        AddSchemaDependencyToOperationIfNecessary(apiName, operationDependsOn, operationTemplateRepresentation);
                        ArmEscapeSampleValueIfNecessary(operationTemplateRepresentation);
                    }
                }

                // add to batch if flagged
                string batchdependsOn;

                if (extractorParameters.OperationBatchSize > 0 && opIndex > 0)
                {
                    if (opIndex >= 1 && opIndex <= numBatches - 1)
                    {
                        // chain the owners to each other
                        batchdependsOn = $"[resourceId('Microsoft.ApiManagement/service/apis/operations', parameters('{ParameterNames.ApimServiceName}'), '{apiName}', '{batchOwners[opIndex - 1]}')]";
                        //Console.WriteLine("Owner chaining: this request {0} to previous {1}", operationName, batchOwners[opIndex-1]);
                    }
                    else
                    {
                        // chain the operation to respective owner
                        int ownerIndex = (int)Math.Floor((opIndex - numBatches) / ((double)extractorParameters.OperationBatchSize - 1));
                        batchdependsOn = $"[resourceId('Microsoft.ApiManagement/service/apis/operations', parameters('{ParameterNames.ApimServiceName}'), '{apiName}', '{batchOwners[ownerIndex]}')]";
                        //Console.WriteLine("Operation {0} chained to owner {1}", operationName, batchOwners[ownerIndex]);
                    }

                    operationDependsOn.Add(batchdependsOn);
                }

                operationResource.DependsOn = operationDependsOn.ToArray();
                templateResources.Add(operationResource);

                // add operation policy resource to api template
                try
                {
                    string operationPolicy = await this.GetOperationPolicyAsync(apimname, resourceGroup, apiName, operationName);
                    Console.WriteLine($" - Operation policy found for {operationName} operation");
                    PolicyTemplateResource operationPolicyResource = JsonConvert.DeserializeObject<PolicyTemplateResource>(operationPolicy);
                    operationPolicyResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{operationResourceName}/policy')]";
                    operationPolicyResource.ApiVersion = GlobalConstants.ApiVersion;
                    operationPolicyResource.Scale = null;
                    operationPolicyResource.DependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis/operations', parameters('{ParameterNames.ApimServiceName}'), '{apiName}', '{operationResourceName}')]" };

                    // write policy xml content to file and point to it if policyXMLBaseUrl is provided
                    if (policyXMLBaseUrl != null)
                    {
                        string policyXMLContent = operationPolicyResource.Properties.Value;
                        string policyFolder = string.Concat(fileFolder, $@"/policies");
                        string operationPolicyFileName = $@"/{apiName}-{operationName}-operationPolicy.xml";
                        FileWriter.CreateFolderIfNotExists(policyFolder);
                        FileWriter.WriteXMLToFile(policyXMLContent, string.Concat(policyFolder, operationPolicyFileName));
                        operationPolicyResource.Properties.Format = "rawxml-link";
                        if (policyXMLSasToken != null)
                        {
                            operationPolicyResource.Properties.Value = $"[concat(parameters('{ParameterNames.PolicyXMLBaseUrl}'), '{operationPolicyFileName}', parameters('{ParameterNames.PolicyXMLSasToken}'))]";
                        }
                        else
                        {
                            operationPolicyResource.Properties.Value = $"[concat(parameters('{ParameterNames.PolicyXMLBaseUrl}'), '{operationPolicyFileName}')]";
                        }
                    }

                    templateResources.Add(operationPolicyResource);
                }
                catch (Exception) { }


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
                        operationTagResource.DependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis/operations', parameters('{ParameterNames.ApimServiceName}'), '{apiName}', '{operationResourceName}')]" };
                        templateResources.Add(operationTagResource);
                    }
                }
                catch (Exception) { }
            }
            #endregion

            #region API Policies
            // add api policy resource to api template
            try
            {
                string apiPolicies = await this.GetAPIPolicyAsync(apimname, resourceGroup, apiName);
                Console.WriteLine("API policy found");
                PolicyTemplateResource apiPoliciesResource = JsonConvert.DeserializeObject<PolicyTemplateResource>(apiPolicies);

                apiPoliciesResource.ApiVersion = GlobalConstants.ApiVersion;
                apiPoliciesResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{apiPoliciesResource.Name}')]";
                apiPoliciesResource.DependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiName}')]" };

                // write policy xml content to file and point to it if policyXMLBaseUrl is provided
                if (policyXMLBaseUrl != null)
                {
                    string policyXMLContent = apiPoliciesResource.Properties.Value;
                    string policyFolder = string.Concat(fileFolder, $@"/policies");
                    string apiPolicyFileName = $@"/{apiName}-apiPolicy.xml";
                    FileWriter.CreateFolderIfNotExists(policyFolder);
                    FileWriter.WriteXMLToFile(policyXMLContent, string.Concat(policyFolder, apiPolicyFileName));
                    apiPoliciesResource.Properties.Format = "rawxml-link";
                    if (policyXMLSasToken != null)
                    {
                        apiPoliciesResource.Properties.Value = $"[concat(parameters('{ParameterNames.PolicyXMLBaseUrl}'), '{apiPolicyFileName}', parameters('{ParameterNames.PolicyXMLSasToken}'))]";
                    }
                    else
                    {
                        apiPoliciesResource.Properties.Value = $"[concat(parameters('{ParameterNames.PolicyXMLBaseUrl}'), '{apiPolicyFileName}')]";
                    }
                }
                templateResources.Add(apiPoliciesResource);
            }
            catch (Exception) { }
            #endregion

            #region API Tags				
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
                    apiTagResource.DependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiName}')]" };
                    templateResources.Add(apiTagResource);
                }
            }
            catch (Exception) { }
            #endregion


            // add product api associations to template
            #region API Products
            try
            {
                // pull product api associations
                string apiProducts = await this.GetAPIProductsAsync(apimname, resourceGroup, apiName);
                JObject oApiProducts = JObject.Parse(apiProducts);

                foreach (var item in oApiProducts["value"])
                {
                    string apiProductName = ((JValue)item["name"]).Value.ToString();
                    Console.WriteLine("'{0}' Product association found", apiProductName);

                    // convert returned api product associations to template resource class
                    ProductAPITemplateResource productAPIResource = JsonConvert.DeserializeObject<ProductAPITemplateResource>(item.ToString());
                    productAPIResource.Type = ResourceTypeConstants.ProductAPI;
                    productAPIResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiProductName}/{apiName}')]";
                    productAPIResource.ApiVersion = GlobalConstants.ApiVersion;
                    productAPIResource.Scale = null;
                    productAPIResource.DependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiName}')]" };

                    templateResources.Add(productAPIResource);
                }
            }
            catch (Exception) { }
            #endregion

            #region Diagnostics
            // add diagnostics to template
            // pull diagnostics for api
            string diagnostics = await this.GetApiDiagnosticsAsync(apimname, resourceGroup, apiName);
            JObject oDiagnostics = JObject.Parse(diagnostics);
            foreach (var diagnostic in oDiagnostics["value"])
            {
                string diagnosticName = ((JValue)diagnostic["name"]).Value.ToString();
                Console.WriteLine("'{0}' Diagnostic found", diagnosticName);

                // convert returned diagnostic to template resource class
                DiagnosticTemplateResource diagnosticResource = diagnostic.ToObject<DiagnosticTemplateResource>();
                diagnosticResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{diagnosticName}')]";
                diagnosticResource.Type = ResourceTypeConstants.APIDiagnostic;
                diagnosticResource.ApiVersion = GlobalConstants.ApiVersion;
                diagnosticResource.Scale = null;
                diagnosticResource.DependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiName}')]" };

                if (extractorParameters.ToParameterizeApiLoggerId)
                {

                    diagnosticResource.Properties.loggerId = $"[parameters('{ParameterNames.ApiLoggerId}').{ParameterNamingHelper.GenerateValidParameterName(apiName, ParameterPrefix.Api)}.{ParameterNamingHelper.GenerateValidParameterName(diagnosticName, ParameterPrefix.Diagnostic)}]";
                }

                if (!diagnosticName.Contains("applicationinsights"))
                {
                    // enableHttpCorrelationHeaders only works for application insights, causes errors otherwise
                    diagnosticResource.Properties.enableHttpCorrelationHeaders = null;
                }

                templateResources.Add(diagnosticResource);

            }
            #endregion

            return templateResources;
        }

        /// <summary>
        /// Gets the "All API" level diagnostic resources, these are common to all APIs.
        /// </summary>
        /// <param name="extractorParameters">The extractor.</param>
        /// <returns>a list of DiagnosticTemplateResources</returns>
        async Task<IEnumerable<TemplateResource>> GetServiceDiagnosticsTemplateResourcesAsync(ExtractorParameters extractorParameters)
        {
            List<TemplateResource> templateResources = new List<TemplateResource>();
            string apimname = extractorParameters.SourceApimName, resourceGroup = extractorParameters.ResourceGroup;

            string serviceDiagnostics = await this.GetServiceDiagnosticsAsync(apimname, resourceGroup);
            JObject oServiceDiagnostics = JObject.Parse(serviceDiagnostics);

            foreach (var serviceDiagnostic in oServiceDiagnostics["value"])
            {
                string serviceDiagnosticName = ((JValue)serviceDiagnostic["name"]).Value.ToString();
                Console.WriteLine("'{0}' Diagnostic found", serviceDiagnosticName);

                // convert returned diagnostic to template resource class
                DiagnosticTemplateResource serviceDiagnosticResource = serviceDiagnostic.ToObject<DiagnosticTemplateResource>();
                serviceDiagnosticResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{serviceDiagnosticName}')]";
                serviceDiagnosticResource.Type = ResourceTypeConstants.APIServiceDiagnostic;
                serviceDiagnosticResource.ApiVersion = GlobalConstants.ApiVersion;
                serviceDiagnosticResource.Scale = null;
                serviceDiagnosticResource.DependsOn = new string[] { };

                if (extractorParameters.ToParameterizeApiLoggerId)
                {

                    serviceDiagnosticResource.Properties.loggerId = $"[parameters('{ParameterNames.ApiLoggerId}').{ParameterNamingHelper.GenerateValidParameterName(serviceDiagnosticName, ParameterPrefix.Diagnostic)}]";
                }

                if (!serviceDiagnosticName.Contains("applicationinsights"))
                {
                    // enableHttpCorrelationHeaders only works for application insights, causes errors otherwise
                    //TODO: Check this settings still valid?
                    serviceDiagnosticResource.Properties.enableHttpCorrelationHeaders = null;
                }

                templateResources.Add(serviceDiagnosticResource);
            }

            return templateResources.ToArray();
        }

        

        static List<TemplateResource> RemoveResourceType(string resourceType, List<TemplateResource> resources)
        {
            List<TemplateResource> newResourcesList = new List<TemplateResource>();
            foreach (TemplateResource resource in resources)
            {
                if (!resource.Type.Equals(resourceType))
                {
                    newResourcesList.Add(resource);
                }
            }
            return newResourcesList;
        }
    }
}
