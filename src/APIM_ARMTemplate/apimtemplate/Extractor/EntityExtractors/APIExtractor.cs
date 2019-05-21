using System;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class APIExtractor : EntityExtractor
    {
        private FileWriter fileWriter;

        public APIExtractor(FileWriter fileWriter)
        {
            this.fileWriter = fileWriter;
        }

        public async Task<string> GetAPIOperationsAsync(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations?api-version={5}",
               baseUrl, azSubId, ResourceGroupName, ApiManagementName, ApiName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
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

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations/{5}/policies/policy?api-version={6}",
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

        public async Task<string> GetAPIsAsync(string ApiManagementName, string ResourceGroupName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis?api-version={4}",
                baseUrl, azSubId, ResourceGroupName, ApiManagementName, GlobalConstants.APIVersion);

            return await CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<string> GetAPIPolicyAsync(string ApiManagementName, string ResourceGroupName, string ApiName)
        {
            (string azToken, string azSubId) = await auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/policies/policy?api-version={5}",
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

        public async Task<Template> GenerateAPIsARMTemplateAsync(string apimname, string resourceGroup, string singleApiName, string policyXMLBaseUrl, string fileFolder)
        {
            // pull all apis from service
            string apis = await GetAPIsAsync(apimname, resourceGroup);
            // initialize arm template
            Template armTemplate = GenerateEmptyTemplateWithParameters(policyXMLBaseUrl);

            JObject oApi = JObject.Parse(apis);
            oApi = FormatoApi(singleApiName, oApi);

            Console.WriteLine("{0} API's found ...", ((JContainer)oApi["value"]).Count.ToString());

            List<TemplateResource> templateResources = new List<TemplateResource>();

            for (int i = 0; i < ((JContainer)oApi["value"]).Count; i++)
            {
                string apiName = ((JValue)oApi["value"][i]["name"]).Value.ToString();
                string apiDetails = await GetAPIDetailsAsync(apimname, resourceGroup, apiName);

                Console.WriteLine("------------------------------------------");
                Console.WriteLine("Extracting resources from {0} API:", apiName);

                // convert returned api to template resource class
                JObject oApiDetails = JObject.Parse(apiDetails);
                APITemplateResource apiResource = JsonConvert.DeserializeObject<APITemplateResource>(apiDetails);
                string oApiName = ((JValue)oApiDetails["name"]).Value.ToString();

                apiResource.type = ((JValue)oApiDetails["type"]).Value.ToString();
                apiResource.name = $"[concat(parameters('ApimServiceName'), '/{oApiName}')]";
                apiResource.apiVersion = GlobalConstants.APIVersion;
                apiResource.scale = null;

                if (apiResource.properties.apiVersionSetId != null)
                {
                    apiResource.dependsOn = new string[] { };

                    string versionSetName = apiResource.properties.apiVersionSetId;
                    int versionSetPosition = versionSetName.IndexOf("apiVersionSets/");

                    versionSetName = versionSetName.Substring(versionSetPosition, (versionSetName.Length - versionSetPosition));
                    apiResource.properties.apiVersionSetId = $"[concat(resourceId('Microsoft.ApiManagement/service', parameters('ApimServiceName')), '/{versionSetName}')]";
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
                string operations = await GetAPIOperationsAsync(apimname, resourceGroup, apiName);
                JObject oOperations = JObject.Parse(operations);

                foreach (var item in oOperations["value"])
                {
                    string operationName = ((JValue)item["name"]).Value.ToString();
                    string operationDetails = await GetAPIOperationDetailsAsync(apimname, resourceGroup, apiName, operationName);

                    Console.WriteLine("'{0}' Operation found", operationName);

                    // convert returned operation to template resource class
                    OperationTemplateResource operationResource = JsonConvert.DeserializeObject<OperationTemplateResource>(operationDetails);
                    string operationResourceName = operationResource.name;
                    operationResource.name = $"[concat(parameters('ApimServiceName'), '/{oApiName}/{operationResourceName}')]";
                    operationResource.apiVersion = GlobalConstants.APIVersion;
                    operationResource.scale = null;

                    // add api and schemas to operation dependsOn, if necessary
                    List<string> operationDependsOn = new List<string>() { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{oApiName}')]" };
                    foreach (OperationTemplateRepresentation operationTemplateRepresentation in operationResource.properties.request.representations)
                    {
                        if (operationTemplateRepresentation.schemaId != null)
                        {
                            string dependsOn = $"[resourceId('Microsoft.ApiManagement/service/apis/schemas', parameters('ApimServiceName'), '{oApiName}', '{operationTemplateRepresentation.schemaId}')]";
                            // add value to list if schema has not already been added
                            if (!operationDependsOn.Exists(o => o == dependsOn))
                            {
                                operationDependsOn.Add(dependsOn);
                            }
                        }
                    }
                    foreach (OperationsTemplateResponse operationTemplateResponse in operationResource.properties.responses)
                    {
                        foreach (OperationTemplateRepresentation operationTemplateRepresentation in operationTemplateResponse.representations)
                        {
                            if (operationTemplateRepresentation.schemaId != null)
                            {
                                string dependsOn = $"[resourceId('Microsoft.ApiManagement/service/apis/schemas', parameters('ApimServiceName'), '{oApiName}', '{operationTemplateRepresentation.schemaId}')]";
                                // add value to list if schema has not already been added
                                if (!operationDependsOn.Exists(o => o == dependsOn))
                                {
                                    operationDependsOn.Add(dependsOn);
                                }
                            }
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
                        operationPolicyResource.name = $"[concat(parameters('ApimServiceName'), '/{oApiName}/{operationResourceName}/policy')]";
                        operationPolicyResource.apiVersion = GlobalConstants.APIVersion;
                        operationPolicyResource.scale = null;
                        operationPolicyResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis/operations', parameters('ApimServiceName'), '{oApiName}', '{operationResourceName}')]" };

                        // write policy xml content to file and point to it if policyXMLBaseUrl is provided
                        if (policyXMLBaseUrl != null)
                        {
                            string policyXMLContent = operationPolicyResource.properties.value;
                            string policyFolder = String.Concat(fileFolder, $@"/policies");
                            string operationPolicyFileName = $@"/{operationName}-operationPolicy.xml";
                            this.fileWriter.CreateFolderIfNotExists(policyFolder);
                            this.fileWriter.WriteXMLToFile(policyXMLContent, String.Concat(policyFolder, operationPolicyFileName));
                            operationPolicyResource.properties.format = "rawxml-link";
                            operationPolicyResource.properties.value = $"[concat(parameters('PolicyXMLBaseUrl'), '{operationPolicyFileName}')]";
                        }

                        templateResources.Add(operationPolicyResource);
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
                    apiPoliciesResource.name = $"[concat(parameters('ApimServiceName'), '/{oApiName}/{apiPoliciesResource.name}')]";
                    apiPoliciesResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{apiName}')]" };

                    // write policy xml content to file and point to it if policyXMLBaseUrl is provided
                    if (policyXMLBaseUrl != null)
                    {
                        string policyXMLContent = apiPoliciesResource.properties.value;
                        string policyFolder = String.Concat(fileFolder, $@"/policies");
                        string apiPolicyFileName = $@"/{apiName}-apiPolicy.xml";
                        this.fileWriter.CreateFolderIfNotExists(policyFolder);
                        this.fileWriter.WriteXMLToFile(policyXMLContent, String.Concat(policyFolder, apiPolicyFileName));
                        apiPoliciesResource.properties.format = "rawxml-link";
                        apiPoliciesResource.properties.value = $"[concat(parameters('PolicyXMLBaseUrl'), '{apiPolicyFileName}')]";
                    }
                    templateResources.Add(apiPoliciesResource);
                }
                catch (Exception) { }
                #endregion

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
                        productAPIResource.name = $"[concat(parameters('ApimServiceName'), '/{apiProductName}/{oApiName}')]";
                        productAPIResource.apiVersion = GlobalConstants.APIVersion;
                        productAPIResource.scale = null;
                        productAPIResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{oApiName}')]" };

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
                    diagnosticResource.name = $"[concat(parameters('ApimServiceName'), '/{oApiName}/{diagnosticName}')]";
                    diagnosticResource.type = ResourceTypeConstants.APIDiagnostic;
                    diagnosticResource.apiVersion = GlobalConstants.APIVersion;
                    diagnosticResource.scale = null;
                    diagnosticResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{oApiName}')]" };

                    if (!diagnosticName.Contains("applicationinsights"))
                    {
                        // enableHttpCorrelationHeaders only works for application insights, causes errors otherwise
                        diagnosticResource.properties.enableHttpCorrelationHeaders = null;
                    }

                    templateResources.Add(diagnosticResource);

                }
                #endregion
            }

            armTemplate.resources = templateResources.ToArray();
            return armTemplate;
        }

        public static JObject FormatoApi(string singleApiName, JObject oApi)
        {
            if (singleApiName != null)
            {
                string json = @"{ 'value': [] }";

                JObject value = JObject.Parse(json);
                JArray item2 = new JArray();
                var objectSelector = string.Format("$.value[?(@.name == '{0}')]", singleApiName);
                var selectedApi = (JObject)oApi.SelectTokens(objectSelector).FirstOrDefault();
                if (selectedApi == null)
                {
                    throw new Exception($"{singleApiName} API not found!");
                }
                item2.Add(selectedApi);
                value["value"] = item2;
                oApi = value;
            }

            return oApi;
        }

        public async Task<List<TemplateResource>> GenerateSchemasARMTemplate(string apimServiceName, string apiName, string resourceGroup, string fileFolder)
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
                schemaDetailsResource.properties.document.value = JsonConvert.SerializeObject(restReturnedSchemaTemplate.properties.document);
                schemaDetailsResource.name = $"[concat(parameters('ApimServiceName'), '/{apiName}/{schemaName}')]";
                schemaDetailsResource.apiVersion = GlobalConstants.APIVersion;
                schemaDetailsResource.dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{apiName}')]" };

                templateResources.Add(schemaDetailsResource);

            }
            return templateResources;
        }
    }
}