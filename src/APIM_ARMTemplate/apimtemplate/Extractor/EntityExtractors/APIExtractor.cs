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
    public class APIExtractor : EntityExtractor
    {
        private FileWriter fileWriter;

        public APIExtractor(FileWriter fileWriter)
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

        public async Task<List<TemplateResource>> GenerateSingleAPIResourceAsync(string apiName, Extractor exc)
        {
            List<TemplateResource> templateResources = new List<TemplateResource>();
            string apimname = exc.sourceApimName, resourceGroup = exc.resourceGroup, fileFolder = exc.fileFolder, policyXMLBaseUrl = exc.policyXMLBaseUrl, policyXMLSasToken = exc.policyXMLSasToken;
            string apiDetails = await GetAPIDetailsAsync(apimname, resourceGroup, apiName);

            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting resources from {0} API:", apiName);

            // convert returned api to template resource class
            JObject oApiDetails = JObject.Parse(apiDetails);
            APITemplateResource apiResource = JsonConvert.DeserializeObject<APITemplateResource>(apiDetails);
            string oApiName = ((JValue)oApiDetails["name"]).Value.ToString();

            apiResource.type = ((JValue)oApiDetails["type"]).Value.ToString();
            apiResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{oApiName}')]";
            apiResource.apiVersion = GlobalConstants.APIVersion;
            apiResource.scale = null;

            if (exc.paramServiceUrl)
            {
                apiResource.properties.serviceUrl = $"[parameters('{ParameterNames.ServiceUrl}').{ExtractorUtils.GenValidParamName(apiName, ParameterPrefix.Api)}]";
            }

            if (apiResource.properties.apiVersionSetId != null)
            {
                apiResource.dependsOn = new string[] { };

                string versionSetName = apiResource.properties.apiVersionSetId;
                int versionSetPosition = versionSetName.IndexOf("apiVersionSets/");

                versionSetName = versionSetName.Substring(versionSetPosition, (versionSetName.Length - versionSetPosition));
                apiResource.properties.apiVersionSetId = $"[concat(resourceId('Microsoft.ApiManagement/service', parameters('{ParameterNames.ApimServiceName}')), '/{versionSetName}')]";
            }
            else
            {
                apiResource.dependsOn = new string[] { };
            }

            templateResources.Add(apiResource);

            #region Schemas
            // add schema resources to api template
            List<TemplateResource> schemaResources = await GenerateSchemasARMTemplate(apimname, apiName, resourceGroup, fileFolder);
            templateResources.AddRange(schemaResources);
            #endregion

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
                operationResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{oApiName}/{operationResourceName}')]";
                operationResource.apiVersion = GlobalConstants.APIVersion;
                operationResource.scale = null;

                // add operation dependencies and fix sample value if necessary
                List<string> operationDependsOn = new List<string>() { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{oApiName}')]" };
                foreach (OperationTemplateRepresentation operationTemplateRepresentation in operationResource.properties.request.representations)
                {
                    AddSchemaDependencyToOperationIfNecessary(oApiName, operationDependsOn, operationTemplateRepresentation);
                    ArmEscapeSampleValueIfNecessary(operationTemplateRepresentation);
                }

                foreach (OperationsTemplateResponse operationTemplateResponse in operationResource.properties.responses)
                {
                    foreach (OperationTemplateRepresentation operationTemplateRepresentation in operationTemplateResponse.representations)
                    {
                        AddSchemaDependencyToOperationIfNecessary(oApiName, operationDependsOn, operationTemplateRepresentation);
                        ArmEscapeSampleValueIfNecessary(operationTemplateRepresentation);
                    }
                }

                operationResource.dependsOn = operationDependsOn.ToArray();
                templateResources.Add(operationResource);

                // add operation policy resource to api template
                try
                {
                    string operationPolicy = await GetOperationPolicyAsync(apimname, resourceGroup, oApiName, operationName);
                    Console.WriteLine($" - Operation policy found for {operationName} operation");
                    PolicyTemplateResource operationPolicyResource = JsonConvert.DeserializeObject<PolicyTemplateResource>(operationPolicy);
                    operationPolicyResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{oApiName}/{operationResourceName}/policy')]";
                    operationPolicyResource.apiVersion = GlobalConstants.APIVersion;
                    operationPolicyResource.scale = null;
                    operationPolicyResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis/operations', parameters('{ParameterNames.ApimServiceName}'), '{oApiName}', '{operationResourceName}')]" };

                    // write policy xml content to file and point to it if policyXMLBaseUrl is provided
                    if (policyXMLBaseUrl != null)
                    {
                        string policyXMLContent = operationPolicyResource.properties.value;
                        string policyFolder = String.Concat(fileFolder, $@"/policies");
                        string operationPolicyFileName = $@"/{apiName}-{operationName}-operationPolicy.xml";
                        this.fileWriter.CreateFolderIfNotExists(policyFolder);
                        this.fileWriter.WriteXMLToFile(policyXMLContent, String.Concat(policyFolder, operationPolicyFileName));
                        operationPolicyResource.properties.format = "rawxml-link";
                        if (policyXMLSasToken != null)
                        {
                            operationPolicyResource.properties.value = $"[concat(parameters('{ParameterNames.PolicyXMLBaseUrl}'), '{operationPolicyFileName}', parameters('{ParameterNames.PolicyXMLSasToken}'))]";
                        }
                        else
                        {
                            operationPolicyResource.properties.value = $"[concat(parameters('{ParameterNames.PolicyXMLBaseUrl}'), '{operationPolicyFileName}')]";
                        }
                    }

                    templateResources.Add(operationPolicyResource);
                }
                catch (Exception) { }


                // add tags associated with the operation to template 
                try
                {
                    // pull tags associated with the operation
                    string apiOperationTags = await GetOperationTagsAsync(apimname, resourceGroup, oApiName, operationName);
                    JObject oApiOperationTags = JObject.Parse(apiOperationTags);

                    foreach (var tag in oApiOperationTags["value"])
                    {
                        string apiOperationTagName = ((JValue)tag["name"]).Value.ToString();
                        Console.WriteLine(" - '{0}' Tag association found for {1} operation", apiOperationTagName, operationResourceName);

                        // convert operation tag association to template resource class
                        TagTemplateResource operationTagResource = JsonConvert.DeserializeObject<TagTemplateResource>(tag.ToString());
                        operationTagResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{oApiName}/{operationResourceName}/{apiOperationTagName}')]";
                        operationTagResource.apiVersion = GlobalConstants.APIVersion;
                        operationTagResource.scale = null;
                        operationTagResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis/operations', parameters('{ParameterNames.ApimServiceName}'), '{oApiName}', '{operationResourceName}')]" };
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
                string apiPolicies = await GetAPIPolicyAsync(apimname, resourceGroup, apiName);
                Console.WriteLine("API policy found");
                PolicyTemplateResource apiPoliciesResource = JsonConvert.DeserializeObject<PolicyTemplateResource>(apiPolicies);

                apiPoliciesResource.apiVersion = GlobalConstants.APIVersion;
                apiPoliciesResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{oApiName}/{apiPoliciesResource.name}')]";
                apiPoliciesResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiName}')]" };

                // write policy xml content to file and point to it if policyXMLBaseUrl is provided
                if (policyXMLBaseUrl != null)
                {
                    string policyXMLContent = apiPoliciesResource.properties.value;
                    string policyFolder = String.Concat(fileFolder, $@"/policies");
                    string apiPolicyFileName = $@"/{apiName}-apiPolicy.xml";
                    this.fileWriter.CreateFolderIfNotExists(policyFolder);
                    this.fileWriter.WriteXMLToFile(policyXMLContent, String.Concat(policyFolder, apiPolicyFileName));
                    apiPoliciesResource.properties.format = "rawxml-link";
                    if (policyXMLSasToken != null)
                    {
                        apiPoliciesResource.properties.value = $"[concat(parameters('{ParameterNames.PolicyXMLBaseUrl}'), '{apiPolicyFileName}', parameters('{ParameterNames.PolicyXMLSasToken}'))]";
                    }
                    else
                    {
                        apiPoliciesResource.properties.value = $"[concat(parameters('{ParameterNames.PolicyXMLBaseUrl}'), '{apiPolicyFileName}')]";
                    }
                }
                templateResources.Add(apiPoliciesResource);
            }
            catch (Exception) { }
            #endregion

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
                    apiTagResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{oApiName}/{apiTagName}')]";
                    apiTagResource.apiVersion = GlobalConstants.APIVersion;
                    apiTagResource.scale = null;
                    apiTagResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{oApiName}')]" };
                    templateResources.Add(apiTagResource);
                }
            }
            catch (Exception) { }

            // add product api associations to template
            #region API Products
            try
            {
                // pull product api associations
                string apiProducts = await GetAPIProductsAsync(apimname, resourceGroup, apiName);
                JObject oApiProducts = JObject.Parse(apiProducts);

                foreach (var item in oApiProducts["value"])
                {
                    string apiProductName = ((JValue)item["name"]).Value.ToString();
                    Console.WriteLine("'{0}' Product association found", apiProductName);

                    // convert returned api product associations to template resource class
                    ProductAPITemplateResource productAPIResource = JsonConvert.DeserializeObject<ProductAPITemplateResource>(item.ToString());
                    productAPIResource.type = ResourceTypeConstants.ProductAPI;
                    productAPIResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiProductName}/{oApiName}')]";
                    productAPIResource.apiVersion = GlobalConstants.APIVersion;
                    productAPIResource.scale = null;
                    productAPIResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{oApiName}')]" };

                    templateResources.Add(productAPIResource);
                }
            }
            catch (Exception) { }
            #endregion

            #region Diagnostics
            // add diagnostics to template
            // pull diagnostics for api
            string diagnostics = await GetAPIDiagnosticsAsync(apimname, resourceGroup, apiName);
            JObject oDiagnostics = JObject.Parse(diagnostics);
            foreach (var diagnostic in oDiagnostics["value"])
            {
                string diagnosticName = ((JValue)diagnostic["name"]).Value.ToString();
                Console.WriteLine("'{0}' Diagnostic found", diagnosticName);

                // convert returned diagnostic to template resource class
                DiagnosticTemplateResource diagnosticResource = diagnostic.ToObject<DiagnosticTemplateResource>();
                diagnosticResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{oApiName}/{diagnosticName}')]";
                diagnosticResource.type = ResourceTypeConstants.APIDiagnostic;
                diagnosticResource.apiVersion = GlobalConstants.APIVersion;
                diagnosticResource.scale = null;
                diagnosticResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{oApiName}')]" };

                if (exc.paramApiLoggerId) {
                    diagnosticResource.properties.loggerId = $"[parameters('{ParameterNames.ApiLoggerId}').{ExtractorUtils.GenValidParamName(apiName, ParameterPrefix.Api)}.{ExtractorUtils.GenValidParamName(diagnosticName, ParameterPrefix.Diagnostic)}]";
                }

                if (!diagnosticName.Contains("applicationinsights"))
                {
                    // enableHttpCorrelationHeaders only works for application insights, causes errors otherwise
                    diagnosticResource.properties.enableHttpCorrelationHeaders = null;
                }

                templateResources.Add(diagnosticResource);

            }
            #endregion
            return templateResources;
        }

        // this function generate apiTemplate for single api with all its revisions
        public async Task<Template> GenerateAPIRevisionTemplateAsync(string currentRevision, List<string> revList, string apiName, Extractor exc)
        {
            // generate apiTemplate
            Template armTemplate = GenerateEmptyTemplateWithParameters(exc.policyXMLBaseUrl, exc.policyXMLSasToken);
            List<TemplateResource> templateResources = new List<TemplateResource>();
            Console.WriteLine("{0} APIs found ...", revList.Count().ToString());

            List<TemplateResource> apiResources = await GenerateSingleAPIResourceAsync(apiName, exc);
            templateResources.AddRange(apiResources);

            foreach (string curApi in revList)
            {
                // should add current api to dependsOn to those revisions that are not "current"
                if (curApi.Equals(currentRevision))
                {
                    // add current API revision resource to template
                    apiResources = await GenerateCurrentRevisionAPIResourceAsync(curApi, exc);
                    templateResources.AddRange(apiResources);
                }
                else
                {
                    // add other API revision resources to template
                    apiResources = await GenerateSingleAPIResourceAsync(curApi, exc);

                    // make current API a dependency to other revisions, in case destination apim doesn't have the this API 
                    TemplateResource apiResource = apiResources.FirstOrDefault(resource => resource.type == ResourceTypeConstants.API) as TemplateResource;
                    List<TemplateResource> newResourcesList = ExtractorUtils.removeResourceType(ResourceTypeConstants.API, apiResources);
                    List<string> dependsOn = apiResource.dependsOn.ToList();
                    dependsOn.Add($"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiName}')]");
                    apiResource.dependsOn = dependsOn.ToArray();
                    newResourcesList.Add(apiResource);

                    templateResources.AddRange(newResourcesList);
                }
            }

            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }

        // this function will get the current revision of this api and will remove "isCurrent" paramter
        public async Task<List<TemplateResource>> GenerateCurrentRevisionAPIResourceAsync(string apiName, Extractor exc)
        {
            List<TemplateResource> templateResources = new List<TemplateResource>();
            string apimname = exc.sourceApimName, resourceGroup = exc.resourceGroup, fileFolder = exc.fileFolder, policyXMLBaseUrl = exc.policyXMLBaseUrl, policyXMLSasToken = exc.policyXMLSasToken;
            string apiDetails = await GetAPIDetailsAsync(apimname, resourceGroup, apiName);
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("Extracting resources from {0} API:", apiName);

            // convert returned api to template resource class
            JObject oApiDetails = JObject.Parse(apiDetails);
            APITemplateResource apiResource = JsonConvert.DeserializeObject<APITemplateResource>(apiDetails);

            apiResource.type = ((JValue)oApiDetails["type"]).Value.ToString();
            apiResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}')]";
            apiResource.apiVersion = GlobalConstants.APIVersion;
            apiResource.scale = null;
            apiResource.properties.isCurrent = null;

            if (exc.paramServiceUrl)
            {
                apiResource.properties.serviceUrl = $"[parameters('{ParameterNames.ServiceUrl}').{ExtractorUtils.GenValidParamName(apiName, ParameterPrefix.Api)}]";
            }

            if (apiResource.properties.apiVersionSetId != null)
            {
                apiResource.dependsOn = new string[] { };

                string versionSetName = apiResource.properties.apiVersionSetId;
                int versionSetPosition = versionSetName.IndexOf("apiVersionSets/");

                versionSetName = versionSetName.Substring(versionSetPosition, (versionSetName.Length - versionSetPosition));
                apiResource.properties.apiVersionSetId = $"[concat(resourceId('Microsoft.ApiManagement/service', parameters('{ParameterNames.ApimServiceName}')), '/{versionSetName}')]";
            }
            else
            {
                apiResource.dependsOn = new string[] { };
            }

            templateResources.Add(apiResource);

            #region Schemas
            // add schema resources to api template
            List<TemplateResource> schemaResources = await GenerateSchemasARMTemplate(apimname, apiName, resourceGroup, fileFolder);
            templateResources.AddRange(schemaResources);
            #endregion

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
                operationResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{operationResourceName}')]";
                operationResource.apiVersion = GlobalConstants.APIVersion;
                operationResource.scale = null;

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

                operationResource.dependsOn = operationDependsOn.ToArray();
                templateResources.Add(operationResource);

                // add operation policy resource to api template
                try
                {
                    string operationPolicy = await GetOperationPolicyAsync(apimname, resourceGroup, apiName, operationName);
                    Console.WriteLine($" - Operation policy found for {operationName} operation");
                    PolicyTemplateResource operationPolicyResource = JsonConvert.DeserializeObject<PolicyTemplateResource>(operationPolicy);
                    operationPolicyResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{operationResourceName}/policy')]";
                    operationPolicyResource.apiVersion = GlobalConstants.APIVersion;
                    operationPolicyResource.scale = null;
                    operationPolicyResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis/operations', parameters('{ParameterNames.ApimServiceName}'), '{apiName}', '{operationResourceName}')]" };

                    // write policy xml content to file and point to it if policyXMLBaseUrl is provided
                    if (policyXMLBaseUrl != null)
                    {
                        string policyXMLContent = operationPolicyResource.properties.value;
                        string policyFolder = String.Concat(fileFolder, $@"/policies");
                        string operationPolicyFileName = $@"/{apiName}-{operationName}-operationPolicy.xml";
                        this.fileWriter.CreateFolderIfNotExists(policyFolder);
                        this.fileWriter.WriteXMLToFile(policyXMLContent, String.Concat(policyFolder, operationPolicyFileName));
                        operationPolicyResource.properties.format = "rawxml-link";
                        if (policyXMLSasToken != null)
                        {
                            operationPolicyResource.properties.value = $"[concat(parameters('{ParameterNames.PolicyXMLBaseUrl}'), '{operationPolicyFileName}', parameters('{ParameterNames.PolicyXMLSasToken}'))]";
                        }
                        else
                        {
                            operationPolicyResource.properties.value = $"[concat(parameters('{ParameterNames.PolicyXMLBaseUrl}'), '{operationPolicyFileName}')]";
                        }
                    }

                    templateResources.Add(operationPolicyResource);
                }
                catch (Exception) { }


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
                        operationTagResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis/operations', parameters('{ParameterNames.ApimServiceName}'), '{apiName}', '{operationResourceName}')]" };
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
                string apiPolicies = await GetAPIPolicyAsync(apimname, resourceGroup, apiName);
                Console.WriteLine("API policy found");
                PolicyTemplateResource apiPoliciesResource = JsonConvert.DeserializeObject<PolicyTemplateResource>(apiPolicies);

                apiPoliciesResource.apiVersion = GlobalConstants.APIVersion;
                apiPoliciesResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{apiPoliciesResource.name}')]";
                apiPoliciesResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiName}')]" };

                // write policy xml content to file and point to it if policyXMLBaseUrl is provided
                if (policyXMLBaseUrl != null)
                {
                    string policyXMLContent = apiPoliciesResource.properties.value;
                    string policyFolder = String.Concat(fileFolder, $@"/policies");
                    string apiPolicyFileName = $@"/{apiName}-apiPolicy.xml";
                    this.fileWriter.CreateFolderIfNotExists(policyFolder);
                    this.fileWriter.WriteXMLToFile(policyXMLContent, String.Concat(policyFolder, apiPolicyFileName));
                    apiPoliciesResource.properties.format = "rawxml-link";
                    if (policyXMLSasToken != null)
                    {
                        apiPoliciesResource.properties.value = $"[concat(parameters('{ParameterNames.PolicyXMLBaseUrl}'), '{apiPolicyFileName}', parameters('{ParameterNames.PolicyXMLSasToken}'))]";
                    }
                    else
                    {
                        apiPoliciesResource.properties.value = $"[concat(parameters('{ParameterNames.PolicyXMLBaseUrl}'), '{apiPolicyFileName}')]";
                    }
                }
                templateResources.Add(apiPoliciesResource);
            }
            catch (Exception) { }
            #endregion

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
                    apiTagResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiName}')]" };
                    templateResources.Add(apiTagResource);
                }
            }
            catch (Exception) { }

            // add product api associations to template
            #region API Products
            try
            {
                // pull product api associations
                string apiProducts = await GetAPIProductsAsync(apimname, resourceGroup, apiName);
                JObject oApiProducts = JObject.Parse(apiProducts);

                foreach (var item in oApiProducts["value"])
                {
                    string apiProductName = ((JValue)item["name"]).Value.ToString();
                    Console.WriteLine("'{0}' Product association found", apiProductName);

                    // convert returned api product associations to template resource class
                    ProductAPITemplateResource productAPIResource = JsonConvert.DeserializeObject<ProductAPITemplateResource>(item.ToString());
                    productAPIResource.type = ResourceTypeConstants.ProductAPI;
                    productAPIResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiProductName}/{apiName}')]";
                    productAPIResource.apiVersion = GlobalConstants.APIVersion;
                    productAPIResource.scale = null;
                    productAPIResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiName}')]" };

                    templateResources.Add(productAPIResource);
                }
            }
            catch (Exception) { }
            #endregion

            #region Diagnostics
            // add diagnostics to template
            // pull diagnostics for api
            string diagnostics = await GetAPIDiagnosticsAsync(apimname, resourceGroup, apiName);
            JObject oDiagnostics = JObject.Parse(diagnostics);
            foreach (var diagnostic in oDiagnostics["value"])
            {
                string diagnosticName = ((JValue)diagnostic["name"]).Value.ToString();
                Console.WriteLine("'{0}' Diagnostic found", diagnosticName);

                // convert returned diagnostic to template resource class
                DiagnosticTemplateResource diagnosticResource = diagnostic.ToObject<DiagnosticTemplateResource>();
                diagnosticResource.name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiName}/{diagnosticName}')]";
                diagnosticResource.type = ResourceTypeConstants.APIDiagnostic;
                diagnosticResource.apiVersion = GlobalConstants.APIVersion;
                diagnosticResource.scale = null;
                diagnosticResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiName}')]" };

                if (exc.paramApiLoggerId) {
                    diagnosticResource.properties.loggerId = $"[parameters('{ParameterNames.ApiLoggerId}').{ExtractorUtils.GenValidParamName(apiName, ParameterPrefix.Api)}.{ExtractorUtils.GenValidParamName(diagnosticName, ParameterPrefix.Diagnostic)}]";
                }

                if (!diagnosticName.Contains("applicationinsights"))
                {
                    // enableHttpCorrelationHeaders only works for application insights, causes errors otherwise
                    diagnosticResource.properties.enableHttpCorrelationHeaders = null;
                }

                templateResources.Add(diagnosticResource);

            }
            #endregion
            return templateResources;
        }

        public async Task<Template> GenerateAPIsARMTemplateAsync(string singleApiName, List<string> multipleApiNames, Extractor exc)
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
                    templateResources.AddRange(await GenerateSingleAPIResourceAsync(singleApiName, exc));
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
                    templateResources.AddRange(await GenerateSingleAPIResourceAsync(apiName, exc));
                }
            }
            // when extract all APIs and generate one master template
            else
            {
                JToken[] oApis = await GetAllAPIObjsAsync(exc.sourceApimName, exc.resourceGroup);
                Console.WriteLine("{0} APIs found ...", (oApis.Count().ToString()));

                foreach (JToken oApi in oApis)
                {
                    string apiName = ((JValue)oApi["name"]).Value.ToString();
                    templateResources.AddRange(await GenerateSingleAPIResourceAsync(apiName, exc));
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